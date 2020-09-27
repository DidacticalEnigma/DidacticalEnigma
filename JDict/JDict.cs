using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using JDict.Xml;
using Optional;
using Optional.Collections;
using Optional.Unsafe;
using TinyIndex;
using Utility.Utils;
using FileMode = System.IO.FileMode;

namespace JDict
{
    public static class JMDict
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(JdicEntry));

        private static readonly XmlReaderSettings xmlSettings = new XmlReaderSettings
        {
            CloseInput = false,
            DtdProcessing = DtdProcessing.Parse, // we have local entities
            XmlResolver = null, // we don't want to resolve against external entities
            MaxCharactersFromEntities = 64 * 1024 * 1024 / 2, // 64 MB
            MaxCharactersInDocument = 256 * 1024 * 1024 / 2, // 256 MB
            IgnoreComments = false
        };

        public static DateTime? GetVersion(Stream stream)
        {
            using (var xmlReader = XmlReader.Create(stream, xmlSettings))
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Comment)
                    {
                        var commentText = xmlReader.Value.Trim();
                        if (commentText.StartsWith("JMdict created:", StringComparison.Ordinal))
                        {
                            var generationDate = commentText.Split(':').ElementAtOrDefault(1)?.Trim();
                            if (DateTime.TryParseExact(generationDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
                            {
                                return date;
                            }

                            return null;
                        }
                    }
                }
            }
            return null;
        }

        public static IEnumerable<JdicEntry> ParseEntries(Stream stream)
        {
            using (var xmlReader = XmlReader.Create(stream, xmlSettings))
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "JMdict")
                    {
                        while (xmlReader.Read())
                        {
                            if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "entry")
                            {
                                using (
                                    var elementReader =
                                        new StringReader(xmlReader.ReadOuterXml()))
                                {
                                    var entry = (JdicEntry)serializer.Deserialize(elementReader);
                                    entry.Senses = entry.Senses ?? Array.Empty<Sense>();
                                    entry.KanjiElements = entry.KanjiElements ?? Array.Empty<KanjiElement>();
                                    entry.ReadingElements = entry.ReadingElements ?? Array.Empty<ReadingElement>();
                                    foreach (var s in entry.Senses)
                                    {
                                        s.Glosses = s.Glosses ?? Array.Empty<Gloss>();
                                        s.PartOfSpeech = s.PartOfSpeech ?? Array.Empty<string>();
                                        s.Antonym = s.Antonym ?? Array.Empty<string>();
                                        s.CrossRef = s.CrossRef ?? Array.Empty<string>();
                                        s.Dialect = s.Dialect ?? Array.Empty<string>();
                                        s.Field = s.Field ?? Array.Empty<string>();
                                        s.Information = s.Information ?? Array.Empty<string>();
                                        s.LoanWordSource = s.LoanWordSource ?? Array.Empty<LoanSource>();
                                        s.Misc = s.Misc ?? Array.Empty<string>();
                                        s.Stagk = s.Stagk ?? Array.Empty<string>();
                                        s.Stagkr = s.Stagkr ?? Array.Empty<string>();
                                        foreach (var gloss in s.Glosses)
                                        {
                                            if (gloss.Lang == null)
                                            {
                                                gloss.Lang = "eng";
                                            }
                                        }

                                        foreach (var loanSource in s.LoanWordSource)
                                        {
                                            if (loanSource.Lang == null)
                                            {
                                                loanSource.Lang = "eng";
                                            }
                                        }
                                    }

                                    yield return entry;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // represents a lookup over an JMdict file
    public class JMDictLookup : IDisposable
    {
        private static readonly Guid Version = new Guid("9D67F5E5-99CB-4FB5-A44B-E27DE67F6684");

        private TinyIndex.Database db;

        private IReadOnlyDiskArray<KeyValuePair<string, IReadOnlyList<long>>> kvps;

        private IReadOnlyDiskArray<JMDictEntry> entries;

        private JMDictLookup Init(Stream stream, string cache)
        {
            var entrySerializer = TinyIndex.Serializer.ForComposite()
                .With(Serializer.ForLong())
                .With(Serializer.ForReadOnlyCollection(Serializer.ForStringAsUTF8()))
                .With(Serializer.ForReadOnlyCollection(Serializer.ForStringAsUTF8()))
                .With(Serializer.ForReadOnlyCollection(Serializer.ForComposite()
                    .With(SerializerExt.ForOption(Serializer.ForEnum<EdictPartOfSpeech>()))
                    .With(Serializer.ForReadOnlyCollection(Serializer.ForEnum<EdictPartOfSpeech>()))
                    .With(Serializer.ForReadOnlyCollection(Serializer.ForEnum<EdictDialect>()))
                    .With(Serializer.ForReadOnlyCollection(Serializer.ForStringAsUTF8()))
                    .With(Serializer.ForReadOnlyCollection(Serializer.ForStringAsUTF8()))
                    .Create()
                    .Mapping(
                        raw => new JMDictSense(
                            (Option<EdictPartOfSpeech>)raw[0],
                            (IReadOnlyCollection<EdictPartOfSpeech>)raw[1],
                            (IReadOnlyCollection<EdictDialect>)raw[2],
                            (IReadOnlyCollection<string>)raw[3],
                            (IReadOnlyCollection<string>)raw[4]),
                        obj => new object[]
                        {
                            obj.Type,
                            obj.PartOfSpeechInfo,
                            obj.DialectalInfo,
                            obj.Glosses,
                            obj.Informational
                        })))
                .Create()
                .Mapping(
                    raw => new JMDictEntry(
                        (long)raw[0],
                        (IReadOnlyCollection<string>)raw[1],
                        (IReadOnlyCollection<string>)raw[2],
                        (IReadOnlyCollection<JMDictSense>)raw[3]),
                    obj => new object[]
                    {
                        obj.SequenceNumber,
                        obj.Readings,
                        obj.Kanji,
                        obj.Senses
                    });

            db = TinyIndex.Database.CreateOrOpen(cache, Version)
                .AddIndirectArray(entrySerializer, db => JMDict.ParseEntries(stream)
                    .Select(CreateEntry),
                        x => x.SequenceNumber)
                .AddIndirectArray(TinyIndex.Serializer.ForKeyValuePair(TinyIndex.Serializer.ForStringAsUTF8(), TinyIndex.Serializer.ForReadOnlyList(TinyIndex.Serializer.ForLong())), db =>
                    {
                        IEnumerable<KeyValuePair<long, string>> It(IEnumerable<JMDictEntry> entries)
                        {
                            foreach (var e in entries)
                            {
                                foreach (var r in e.Kanji)
                                {
                                    yield return new KeyValuePair<long, string>(e.SequenceNumber, r);
                                }

                                foreach (var r in e.Readings)
                                {
                                    yield return new KeyValuePair<long, string>(e.SequenceNumber, r);
                                }
                            }
                        }

                        return It(db.Get<JMDictEntry>(0)
                            .LinearScan())
                            .GroupBy(kvp => kvp.Value, kvp => kvp.Key)
                            .Select(x => new KeyValuePair<string, IReadOnlyList<long>>(x.Key, x.ToList()));
                    },
                    x => x.Key, StringComparer.Ordinal)
                .Build();
            entries = db.Get<JMDictEntry>(0, new LruCache<long, JMDictEntry>(128));
            kvps = db.Get<KeyValuePair<string, IReadOnlyList<long>>>(1, new LruCache<long, KeyValuePair<string, IReadOnlyList<long>>>(128));

            return this;
        }

        private JMDictEntry CreateEntry(JdicEntry xmlEntry)
        {
            return new JMDictEntry(
                xmlEntry.Number,
                xmlEntry.ReadingElements.Select(r => r.Reb).ToList(),
                xmlEntry.KanjiElements.Select(k => k.Key).ToList(),
                CreateSenses(xmlEntry));
        }

        private IReadOnlyCollection<JMDictSense> CreateSenses(JdicEntry xmlEntry)
        {
            var sense = new List<JMDictSense>();
            string[] partOfSpeech = Array.Empty<string>();
            var typedPartOfSpeech = new List<EdictPartOfSpeech>();
            foreach (var s in xmlEntry.Senses)
            {
                if (s.PartOfSpeech.Length > 0)
                {
                    partOfSpeech = s.PartOfSpeech;
                    typedPartOfSpeech = partOfSpeech.Select(posStr => EdictTypeUtils.FromDescription(posStr).ValueOr(() =>
                    {
                        Debug.WriteLine($"{posStr} unknown");
                        return default(EdictPartOfSpeech);
                    })).ToList();
                }

                sense.Add(new JMDictSense(
                    partOfSpeech.Select(EdictTypeUtils.FromDescription).FirstOrNone().Flatten(),
                    typedPartOfSpeech,
                    s.Dialect.Select(EdictDialectUtils.FromDescription).Values().ToList(),
                    s.Glosses.Select(g => g.Text.Trim()).ToList(),
                    s.Information.ToList()));
            }

            return sense;
        }

        private async Task<JMDictLookup> InitAsync(Stream stream, string cache)
        {
            // TODO: not a lazy way
            await Task.Run(() => Init(stream, cache));
            return this;
        }

        public Option<JMDictEntry> LookupBySequenceNumber(long sequenceNumber)
        {
            var searchResult = entries.BinarySearch(sequenceNumber, e => e.SequenceNumber);
            if (searchResult.id != -1)
            {
                return searchResult.element.Some();
            }

            return Option.None<JMDictEntry>();
        }

        public IEnumerable<JMDictEntry> AllEntries()
        {
            return entries.LinearScan();
        }

        public IEnumerable<JMDictEntry> Lookup(string key)
        {
            var res = kvps.BinarySearch(key, kvp => kvp.Key, StringComparer.Ordinal);
            if (res.id == -1)
            {
                return null;
            }
            else
            {
                return It();
            }

            IEnumerable<JMDictEntry> It()
            {
                var sequenceNumbers = res.element.Value;
                foreach (var sequenceNumber in sequenceNumbers)
                {
                    var searchResult = entries.BinarySearch(sequenceNumber, e => e.SequenceNumber);
                    if (searchResult.id != -1)
                    {
                        yield return searchResult.element;
                    }
                }
            }
        }

        private async Task<JMDictLookup> InitAsync(string path, string cache)
        {
            using (var file = File.OpenRead(path))
            using (var gzip = new GZipStream(file, CompressionMode.Decompress))
            {
                return await InitAsync(gzip, cache);
            }
        }

        private JMDictLookup Init(string path, string cache)
        {
            using (var file = File.OpenRead(path))
            using (var gzip = new GZipStream(file, CompressionMode.Decompress))
            {
                return Init(gzip, cache);
            }
        }

        private JMDictLookup()
        {

        }

        public static JMDictLookup Create(string path, string cache)
        {
            return new JMDictLookup().Init(
                path,
                cache);
        }

        public static JMDictLookup Create(Stream stream, string cache)
        {
            return new JMDictLookup().Init(
                stream,
                cache);
        }

        public static async Task<JMDictLookup> CreateAsync(string path, string cache)
        {
            return await new JMDictLookup().InitAsync(
                path,
                cache);
        }

        public static async Task<JMDictLookup> CreateAsync(Stream stream, string cache)
        {
            return await new JMDictLookup().InitAsync(
                stream,
                cache);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }

    public struct PriorityTag
    {
        private int raw;

        private Kind kind;

        private PriorityTag(int raw, Kind kind)
        {
            this.raw = raw;
            this.kind = kind;
        }

        private enum Kind
        {
            none,
            news,
            ichi,
            spec,
            gai,
            nf
        }

        public int? CompareTo(PriorityTag other)
        {
            if (this.kind == Kind.none && other.kind != Kind.none)
            {
                return -1;
            }
            if (this.kind != Kind.none && other.kind == Kind.none)
            {
                return 1;
            }

            if (this.kind != other.kind)
                return null;

            return this.raw.CompareTo(other.raw);
        }

        public static PriorityTag News1 { get; } = new PriorityTag(1, Kind.news);

        public static PriorityTag News2 { get; } = new PriorityTag(2, Kind.news);

        public static PriorityTag Ichi1 { get; } = new PriorityTag(1, Kind.ichi);

        public static PriorityTag Ichi2 { get; } = new PriorityTag(2, Kind.ichi);

        public static PriorityTag Spec1 { get; } = new PriorityTag(1, Kind.spec);

        public static PriorityTag Spec2 { get; } = new PriorityTag(2, Kind.spec);

        public static PriorityTag Gai1 { get; } = new PriorityTag(1, Kind.gai);

        public static PriorityTag Gai2 { get; } = new PriorityTag(2, Kind.gai);

        public static PriorityTag Nf(int rating) => new PriorityTag(rating, Kind.nf);

        public static PriorityTag FromString(string str)
        {
            if (TryParse(str, out var tag))
            {
                return tag;
            }
            throw new ArgumentException("Invalid priority tag", nameof(str));


            bool TryParse(string s, out PriorityTag t)
            {
                var firstDigitIndex = s.IndexOfAny(new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'});
                if (firstDigitIndex == -1)
                {
                    t = default;
                    return false;
                }

                if (Enum.TryParse<Kind>(s.Substring(0, firstDigitIndex), out var kind) &&
                    int.TryParse(s.Substring(firstDigitIndex), out var value))
                {
                    t = new PriorityTag(value, kind);
                    return true;
                }

                t = default;
                return false;
            }
        }

        public override string ToString()
        {
            if (kind == Kind.nf)
            {
                return $"{kind}{raw:D2}";
            }

            return $"{kind}{raw}";
        }
    }

    public class JMDictReading
    {
        public string Reading { get; }

        public bool NotATrueReading { get; }
    }

    public class JMDictKanji
    {
        public string Kanji { get; }

        public IEnumerable<string> Informational { get; }
    }

    public class JMDictEntry
    {
        public long SequenceNumber { get; }

        public IEnumerable<string> Readings { get; }

        public IEnumerable<string> Kanji { get; }

        public IEnumerable<JMDictSense> Senses { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            bool first;
            {
                first = true;
                foreach (var kanji in Kanji)
                {
                    if (!first)
                        sb.Append(";  ");
                    first = false;
                    sb.Append(kanji);
                }
                sb.AppendLine();
            }
            {
                first = true;
                foreach (var reading in Readings)
                {
                    if (!first)
                        sb.AppendLine();
                    first = false;
                    sb.Append(reading);
                }
                sb.AppendLine();
            }
            {
                foreach (var sense in Senses)
                {
                    sb.Append(sense);
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        public JMDictEntry(
            long sequenceNumber,
            IReadOnlyCollection<string> readings,
            IReadOnlyCollection<string> kanji,
            IReadOnlyCollection<JMDictSense> senses)
        {
            SequenceNumber = sequenceNumber;
            Readings = readings;
            Kanji = kanji;
            Senses = senses;
        }
    }

    public class JMDictSense
    {
        public Option<EdictPartOfSpeech> Type { get; }

        public IEnumerable<EdictPartOfSpeech> PartOfSpeechInfo { get; }

        public IEnumerable<string> Glosses { get; }

        public IEnumerable<string> Informational { get; }

        private string PartOfSpeechString => string.Join("/", PartOfSpeechInfo.Select(pos => pos.ToDescription()));

        private string Description => string.Join("/", Glosses);

        public IEnumerable<EdictDialect> DialectalInfo { get; }

        public JMDictSense(
            Option<EdictPartOfSpeech> type,
            IReadOnlyCollection<EdictPartOfSpeech> pos,
            IReadOnlyCollection<EdictDialect> dialect,
            IReadOnlyCollection<string> text,
            IReadOnlyCollection<string> informational)
        {
            Type = type;
            PartOfSpeechInfo = pos;
            DialectalInfo = dialect;
            Glosses = text;
            Informational = informational;
        }

        public override string ToString()
        {
            return PartOfSpeechString + "\n" + Description;
        }
    }

    public static class EdictTypeUtils
    {
        public static Option<EdictPartOfSpeech> FromDescription(string description)
        {
            return mapping.FromDescription(description);
        }

        public static string ToDescription(this EdictPartOfSpeech pos)
        {
            return mapping.ToLongString(pos);
        }

        public static string ToAbbrevation(this EdictPartOfSpeech pos)
        {
            return pos.ToString().Replace("_", "-");
        }

        private static EnumMapper<EdictPartOfSpeech> mapping = new EnumMapper<EdictPartOfSpeech>();
    }

    // Make sure to synchronize these constants with the values at LibJpConjSharp project
    public enum EdictPartOfSpeech
    {
        [Description("This means no type at all, it is used to return the radical as it is")]
        v0 = 0,

        [Description("Ichidan verb")]
        v1 = 1,

        [Description("Nidan verb with 'u' ending (archaic)")]
        v2a_s = 2,

        [Description("Yodan verb with `hu/fu' ending (archaic)")]
        v4h = 3,

        [Description("Yodan verb with `ru' ending (archaic)")]
        v4r = 4,

        [Description("Godan verb (not completely classified)")]
        v5 = 5,

        [Description("Godan verb - -aru special class")]
        v5aru = 6,

        [Description("Godan verb with `bu' ending")]
        v5b = 7,

        [Description("Godan verb with `gu' ending")]
        v5g = 8,

        [Description("Godan verb with `ku' ending")]
        v5k = 9,

        [Description("Godan verb - Iku/Yuku special class")]
        v5k_s = 10,

        [Description("Godan verb with `mu' ending")]
        v5m = 11,

        [Description("Godan verb with `nu' ending")]
        v5n = 12,

        [Description("Godan verb with `ru' ending")]
        v5r = 13,

        [Description("Godan verb with `ru' ending (irregular verb)")]
        v5r_i = 14,

        [Description("Godan verb with `su' ending")]
        v5s = 15,

        [Description("Godan verb with `tsu' ending")]
        v5t = 16,

        [Description("Godan verb with `u' ending")]
        v5u = 17,

        [Description("Godan verb with `u' ending (special class)")]
        v5u_s = 18,

        [Description("Godan verb - uru old class verb (old form of Eru)")]
        v5uru = 19,

        [Description("Godan verb with `zu' ending")]
        v5z = 20,

        [Description("Ichidan verb - zuru verb (alternative form of -jiru verbs)")]
        vz = 21,

        [Description("Kuru verb - special class")]
        vk = 22,

        [Description("irregular nu verb")]
        vn = 23,

        [Description("noun or participle which takes the aux. verb suru")]
        vs = 24,

        [Description("su verb - precursor to the modern suru")]
        vs_c = 25,

        [Description("suru verb - included")]
        vs_i = 26,

        [Description("suru verb - special class")]
        vs_s = 27,

        [Description("Yodan verb with `ku' ending (archaic)")]
        v4k,
        [Description("Yodan verb with `gu' ending (archaic)")]
        v4g,
        [Description("Yodan verb with `su' ending (archaic)")]
        v4s,
        [Description("Yodan verb with `tsu' ending (archaic)")]
        v4t,
        [Description("Yodan verb with `nu' ending (archaic)")]
        v4n,
        [Description("Yodan verb with `bu' ending (archaic)")]
        v4b,
        [Description("Yodan verb with `mu' ending (archaic)")]
        v4m,
        [Description("Nidan verb (upper class) with `ku' ending (archaic)")]
        v2k_k,
        [Description("Nidan verb (upper class) with `gu' ending (archaic)")]
        v2g_k,
        [Description("Nidan verb (upper class) with `tsu' ending (archaic)")]
        v2t_k,
        [Description("Nidan verb (upper class) with `dzu' ending (archaic)")]
        v2d_k,
        [Description("Nidan verb (upper class) with `hu/fu' ending (archaic)")]
        v2h_k,
        [Description("Nidan verb (upper class) with `bu' ending (archaic)")]
        v2b_k,
        [Description("Nidan verb (upper class) with `mu' ending (archaic)")]
        v2m_k,
        [Description("Nidan verb (upper class) with `yu' ending (archaic)")]
        v2y_k,
        [Description("Nidan verb (upper class) with `ru' ending (archaic)")]
        v2r_k,
        [Description("Nidan verb (lower class) with `ku' ending (archaic)")]
        v2k_s,
        [Description("Nidan verb (lower class) with `gu' ending (archaic)")]
        v2g_s,
        [Description("Nidan verb (lower class) with `su' ending (archaic)")]
        v2s_s,
        [Description("Nidan verb (lower class) with `zu' ending (archaic)")]
        v2z_s,
        [Description("Nidan verb (lower class) with `tsu' ending (archaic)")]
        v2t_s,
        [Description("Nidan verb (lower class) with `dzu' ending (archaic)")]
        v2d_s,
        [Description("Nidan verb (lower class) with `nu' ending (archaic)")]
        v2n_s,
        [Description("Nidan verb (lower class) with `hu/fu' ending (archaic)")]
        v2h_s,
        [Description("Nidan verb (lower class) with `bu' ending (archaic)")]
        v2b_s,
        [Description("Nidan verb (lower class) with `mu' ending (archaic)")]
        v2m_s,
        [Description("Nidan verb (lower class) with `yu' ending (archaic)")]
        v2y_s,
        [Description("Nidan verb (lower class) with `ru' ending (archaic)")]
        v2r_s,
        [Description("Nidan verb (lower class) with `u' ending and `we' conjugation (archaic)")]
        v2w_s,
        [Description("verb unspecified")]
        v_unspec,
        [Description("irregular verb")]
        iv,
        [Description("Ichidan verb - kureru special class")]
        v1_s,
        [Description("intransitive verb")]
        vi,
        [Description("irregular ru verb, plain form ends with -ri")]
        vr,
        [Description("transitive verb")]
        vt,

        [Description("noun (common) (futsuumeishi)")]
        n = 128,

        [Description("adverbial noun (fukushitekimeishi)")]
        n_adv = 129,

        [Description("noun, used as a suffix")]
        n_suf = 130,

        [Description("noun, used as a prefix")]
        n_pref = 131,

        [Description("noun (temporal) (jisoumeishi)")]
        n_t = 132,

        [Description("particle")]
        prt = 256,

        [Description("pronoun")]
        pn,

        [Description("pre-noun adjectival (rentaishi)")]
        adj_pn,

        [Description("auxiliary verb")]
        aux_v,

        [Description("copula")]
        cop_da,

        [Description("adjective (keiyoushi)")]
        adj_i,
        [Description("adjective (keiyoushi) - yoi/ii class")]
        adj_ix,
        [Description("adjectival nouns or quasi-adjectives (keiyodoshi)")]
        adj_na,
        [Description("nouns which may take the genitive case particle `no'")]
        adj_no,
        [Description("`taru' adjective")]
        adj_t,
        [Description("noun or verb acting prenominally")]
        adj_f,
        [Description("adverb (fukushi)")]
        adv,
        [Description("adverb taking the `to' particle")]
        adv_to,
        [Description("auxiliary")]
        aux,
        [Description("auxiliary adjective")]
        aux_adj,
        [Description("conjunction")]
        conj,
        [Description("counter")]
        ctr,
        [Description("expressions (phrases, clauses, etc.)")]
        exp,
        [Description("interjection (kandoushi)")]
        @int,
        [Description("numeric")]
        num,
        [Description("prefix")]
        pref,
        [Description("suffix")]
        suf,
        [Description("unclassified")]
        unc,
        [Description("`kari' adjective (archaic)")]
        adj_kari,
        [Description("`ku' adjective (archaic)")]
        adj_ku,
        [Description("`shiku' adjective (archaic)")]
        adj_shiku,
        [Description("archaic/formal form of na-adjective")]
        adj_nari,
        [Description("proper noun")]
        n_pr,
    }

    public enum EdictDialect
    {
        // reserving 0 for an "unknown"
        [Description("Kyoto-ben")]
        kyb = 1,
        [Description("Osaka-ben")]
        osb,
        [Description("Kansai-ben")]
        ksb,
        [Description("Kantou-ben")]
        ktb,
        [Description("Tosa-ben")]
        tsb,
        [Description("Touhoku-ben")]
        thb,
        [Description("Tsugaru-ben")]
        tsug,
        [Description("Kyuushuu-ben")]
        kyu,
        [Description("Ryuukyuu-ben")]
        rkb,
        [Description("Hokkaido-ben")]
        hob,
        [Description("Nagano-ben")]
        nab,
    }

    public static class EdictDialectUtils
    {
        public static Option<EdictDialect> FromDescription(string description)
        {
            return mapping.FromDescription(description);
        }

        public static string ToDescription(this EdictDialect d)
        {
            return mapping.ToLongString(d);
        }

        public static string ToAbbrevation(this EdictPartOfSpeech d)
        {
            return d.ToString().Replace("_", "-");
        }

        private static EnumMapper<EdictDialect> mapping = new EnumMapper<EdictDialect>();
    }

    enum EdictField
    {
        // reserving 0 for an "unknown"
        [Description("martial arts term")]
        MA = 1,
        [Description("Buddhist term")]
        Buddh,
        [Description("chemistry term")]
        chem,
        [Description("computer terminology")]
        comp,
        [Description("food term")]
        food,
        [Description("geometry term")]
        geom,
        [Description("linguistics terminology")]
        ling,
        [Description("mathematics")]
        math,
        [Description("military")]
        mil,
        [Description("physics terminology")]
        physics,
        [Description("astronomy, etc. term")]
        astron,
        [Description("baseball term")]
        baseb,
        [Description("biology term")]
        biol,
        [Description("botany term")]
        bot,
        [Description("business term")]
        bus,
        [Description("economics term")]
        econ,
        [Description("engineering term")]
        engr,
        [Description("finance term")]
        finc,
        [Description("geology, etc. term")]
        geol,
        [Description("law, etc. term")]
        law,
        [Description("mahjong term")]
        mahj,
        [Description("medicine, etc. term")]
        med,
        [Description("music term")]
        music,
        [Description("Shinto term")]
        Shinto,
        [Description("shogi term")]
        shogi,
        [Description("sports term")]
        sports,
        [Description("sumo term")]
        sumo,
        [Description("zoology term")]
        zool,
        [Description("anatomical term")]
        anat,
        [Description("Christian term")]
        Christn,
        [Description("Internet slang")]
        net_sl,
        [Description("dated term")]
        dated,
        [Description("historical term")]
        hist,
        [Description("literary or formal term")]
        litf,
        [Description("family or surname")]
        surname,
        [Description("place name")]
        place,
        [Description("unclassified name")]
        unclass,
        [Description("company name")]
        company,
        [Description("product name")]
        product,
        [Description("work of art, literature, music, etc. name")]
        work,
        [Description("full name of a particular person")]
        person,
        [Description("given name or forename, gender not specified")]
        given,
        [Description("railway station")]
        station,
        [Description("organization name")]
        organization,
    }

    enum EdictMisc
    {
        // reserving 0 for an "unknown"
        [Description("architecture term")]
        archit = 1,
        [Description("abbreviation")]
        abbr,
        [Description("archaism")]
        arch,
        [Description("children's language")]
        chn,
        [Description("colloquialism")]
        col,
        [Description("derogatory")]
        derog,
        [Description("familiar language")]
        fam,
        [Description("female term or language")]
        fem,
        [Description("honorific or respectful (sonkeigo) language")]
        hon,
        [Description("humble (kenjougo) language")]
        hum,
        [Description("idiomatic expression")]
        id,
        [Description("manga slang")]
        m_sl,
        [Description("male term or language")]
        male,
        [Description("male slang")]
        male_sl,
        [Description("obsolete term")]
        obs,
        [Description("obscure term")]
        obsc,
        [Description("onomatopoeic or mimetic word")]
        on_mim,
        [Description("poetical term")]
        poet,
        [Description("polite (teineigo) language")]
        pol,
        [Description("proverb")]
        proverb,
        [Description("quotation")]
        quote,
        [Description("rare")]
        rare,
        [Description("sensitive")]
        sens,
        [Description("slang")]
        sl,
        [Description("word usually written using kana alone")]
        uk,
        [Description("yojijukugo")]
        yoji,
        [Description("vulgar expression or word")]
        vulg,
        [Description("jocular, humorous term")]
        joc,
    }

    enum EdictReadingInformation
    {
        // reserving 0 for an "unknown"
        [Description("gikun (meaning as reading) or jukujikun (special kanji reading)")]
        gikun = 1,
        [Description("word containing irregular kana usage")]
        ik,
        [Description("out-dated or obsolete kana usage")]
        ok,
        [Description("old or irregular kana form")]
        oik,
    }

    enum EdictKanjiInformation
    {
        // reserving 0 for an "unknown"
        [Description("ateji (phonetic) reading")]
        ateji = 1,
        [Description("word containing irregular kanji usage")]
        iK,
        [Description("word containing irregular kana usage")]
        ik,
        [Description("irregular okurigana usage")]
        io,
        [Description("word containing out-dated kanji")]
        oK,
    }

    /*
        uncategorized:
        [Description("rude or X-rated term (not displayed in educational software)")]
        X,
        [Description("exclusively kanji")]
        eK,
        [Description("exclusively kana")]
        ek,
        [Description("word usually written using kanji alone")]
        uK,
    */
}