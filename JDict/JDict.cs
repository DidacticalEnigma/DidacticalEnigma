using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using JDict.Internal.XmlModels;
using LiteDB;
using Optional;
using Optional.Collections;
using Optional.Unsafe;
using Utility.Utils;
using FileMode = System.IO.FileMode;

namespace JDict
{
    // represents a lookup over an JMdict file
    public class JMDict : IDisposable
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(JdicRoot));

        private static readonly int Version = 4;

        private LiteDatabase db;

        private LiteCollection<DbDictEntryKeyValue> kvps;

        private LiteCollection<DbDictVersion> version;

        private LiteCollection<DbDictEntry> entries;

        private LiteCollection<DbSense> senses;

        private JMDict Init(Stream stream, LiteDatabase cache)
        {
            db = cache;
            version = cache.GetCollection<DbDictVersion>("version");
            kvps = cache.GetCollection<DbDictEntryKeyValue>("kvps");
            entries = cache.GetCollection<DbDictEntry>("entries");
            senses = cache.GetCollection<DbSense>("senses");
            var versionInfo = version.FindAll().FirstOrDefault();
            if (versionInfo == null ||
                (versionInfo.OriginalFileSize != -1 && stream.CanSeek && stream.Length != versionInfo.OriginalFileSize) ||
                versionInfo.DbVersion != Version)
            {
                var root = ReadFromXml(stream);
                FillDatabase(root);
            }

            return this;
        }

        private IEnumerable<JdicEntry> Deserialize(XmlReader reader)
        {
            return ((JdicRoot) serializer.Deserialize(reader)).Entries;
        }

        private Dictionary<string, List<JMDictEntry>> ReadFromXml(Stream stream)
        {
            var xmlSettings = new XmlReaderSettings
            {
                CloseInput = false,
                DtdProcessing = DtdProcessing.Parse, // we have local entities
                XmlResolver = null, // we don't want to resolve against external entities
                MaxCharactersFromEntities = 64 * 1024 * 1024 / 2, // 64 MB
                MaxCharactersInDocument = 256 * 1024 * 1024 / 2 // 256 MB
            };
            using (var xmlReader = XmlReader.Create(stream, xmlSettings))
            {
                var root = new Dictionary<string, List<JMDictEntry>>();
                var entries = Deserialize(xmlReader)
                    .SelectMany(e =>
                    {
                        var kanjiElements = e.KanjiElements ?? Enumerable.Empty<KanjiElement>();
                        return kanjiElements
                            .Select(k => new KeyValuePair<string, JdicEntry>(k.Key, e))
                            .Concat(e.ReadingElements.Select(r => new KeyValuePair<string, JdicEntry>(r.Reb, e)));
                    });
                foreach (var kvp in entries)
                {
                    var xmlEntry = kvp.Value;
                    if (!root.ContainsKey(kvp.Key))
                        root[kvp.Key] = new List<JMDictEntry>();
                    root[kvp.Key].Add(new JMDictEntry(
                        xmlEntry.Number,
                        xmlEntry.ReadingElements.Select(r => r.Reb),
                        xmlEntry.KanjiElements?.Select(k => k.Key) ?? Enumerable.Empty<string>(),
                        CreateSense(xmlEntry)));
                }

                return root;
            }
        }

        private IEnumerable<JMDictSense> CreateSense(JdicEntry xmlEntry)
        {
            var sense = new List<JMDictSense>();
            string[] previousPartOfSpeech = null;
            foreach (var s in xmlEntry.Senses)
            {
                var partOfSpeech = s.PartOfSpeech ?? previousPartOfSpeech;
                var partOfSpeechString = s.PartOfSpeech != null ? string.Join("/", s.PartOfSpeech) : "";
                sense.Add(new JMDictSense(
                    EdictTypeUtils.FromDescription(partOfSpeech?.FirstOrNone(pos => EdictTypeUtils.FromDescription(pos).HasValue).ValueOr("")),
                    partOfSpeechString,
                    string.Join("/", s.Glosses.Select(g => g.Text.Trim()))));
                previousPartOfSpeech = partOfSpeech;
            }

            return sense;
        }

        private void FillDatabase(IReadOnlyDictionary<string, List<JMDictEntry>> root)
        {
            senses.Delete(_ => true);
            entries.Delete(_ => true);
            kvps.Delete(_ => true);
            version.Delete(_ => true);

            var sensesDict = root.Values
                .SelectMany(e => e.SelectMany(p => p.Senses))
                .Distinct()
                .Select((s, i) => (s, i+2))
                .ToDictionary(kvp => kvp.Item1, kvp => DbSense.From(kvp.Item1, kvp.Item2));

            var entriesDict = root.Values
                .SelectMany(e => e)
                .Distinct()
                .Select((e, i) => (e, i+2))
                .ToDictionary(
                    kvp => kvp.Item1,
                    kvp => DbDictEntry.From(kvp.Item1, s => sensesDict[s], kvp.Item2));

            var kvpsDict = root
                .ToDictionary(kvp => DbDictEntryKeyValue.From(
                    new KeyValuePair<string, IEnumerable<JMDictEntry>>(kvp.Key, kvp.Value),
                    e => entriesDict[e]));

            senses.InsertBulk(sensesDict.Values);
            entries.InsertBulk(entriesDict.Values);
            kvps.InsertBulk(kvpsDict.Select(kvp => DbDictEntryKeyValue.From(
                new KeyValuePair<string, IEnumerable<JMDictEntry>>(kvp.Value.Key, kvp.Value.Value),
                e => entriesDict[e])));

            kvps.EnsureIndex(x => x.LookupKey);

            version.Insert(new DbDictVersion
            {
                DbVersion = Version,
                OriginalFileSize = -1,
                OriginalFileHash = Array.Empty<byte>()
            });
        }

        private async Task<JMDict> InitAsync(Stream stream, LiteDatabase cache)
        {
            // TODO: not a lazy way
            await Task.Run(() => Init(stream, cache));
            return this;
        }

        public IEnumerable<JMDictEntry> Lookup(string v)
        {
            return kvps
                .IncludeAll()
                .FindOne(kvp => kvp.LookupKey == v)
                ?.Values
                ?.Select(e => e.To(s => s.To()));
        }

        public IEnumerable<string> PartialWordLookup(string input)
        {
            string wildcardChar = "/\\";
            IEnumerable<DbDictEntryKeyValue> preFiltered;
            if (input.Replace(wildcardChar, "").Length == input.Length)
            {
                // if there's no placeholders, just do unique search
                var key = kvps.FindOne(kvp => kvp.LookupKey == input);
                return key != null
                    ? EnumerableExt.OfSingle(key.LookupKey).ToList()
                    : new List<string>();
            }
            var components = input
                .Split(new[] { wildcardChar }, StringSplitOptions.RemoveEmptyEntries);
            var regex = new Regex("^" + Regex.Escape(input).Replace(@"/\\", ".") + "$");
            if (components.Length == 0)
            {
                preFiltered = kvps.FindAll();
            }
            else
            {
                if (!input.StartsWith(wildcardChar))
                {
                    // prefix search is preferred because we can do filtering before we even hit the index
                    preFiltered = LookupByStart(components[0]);
                }
                else
                {
                    // worst case we filter by anywhere in the query
                    var x = components
                        .MaxBy(l => l.Length);
                    preFiltered = LookupByAnywhere(x);
                }
            }

            return preFiltered
                .Where(kvp => regex.IsMatch(kvp.LookupKey))
                .Select(kvp => kvp.LookupKey)
                .ToList();
        }

        private IEnumerable<DbDictEntryKeyValue> LookupByLength(string v)
        {
            var len = v.Length;
            return kvps.Find(kvp => kvp.LookupKey.Length == len);
        }

        private IEnumerable<DbDictEntryKeyValue> LookupByStart(string v)
        {
            return kvps.Find(kvp => kvp.LookupKey.StartsWith(v));
        }

        private IEnumerable<DbDictEntryKeyValue> LookupByAnywhere(string v)
        {
            return kvps.Find(kvp => kvp.LookupKey.Contains(v));
        }

        public IEnumerable<string> WordLookupByPredicate(Func<string, bool> matcher)
        {
            return kvps
                .FindAll()
                .Where(kvp => matcher(kvp.LookupKey))
                .Select(kvp => kvp.LookupKey)
                .ToList();
        }

        private KeyValuePair<string, IEnumerable<JMDictEntry>> SimpleMap(DbDictEntryKeyValue kvp)
        {
            return kvp.To(e => e.To(en => en.To()));
        }

        private async Task<JMDict> InitAsync(string path, LiteDatabase cache)
        {
            using (var file = File.OpenRead(path))
            using (var gzip = new GZipStream(file, CompressionMode.Decompress))
            {
                return await InitAsync(gzip, cache);
            }
        }

        private JMDict Init(string path, LiteDatabase cache)
        {
            using (var file = File.OpenRead(path))
            using (var gzip = new GZipStream(file, CompressionMode.Decompress))
            {
                return Init(gzip, cache);
            }
        }

        private JMDict()
        {

        }

        public static JMDict Create(string path, string cache)
        {
            return new JMDict().Init(
                path,
                OpenDatabase(cache));
        }

        public static JMDict Create(Stream stream, string cache)
        {
            return new JMDict().Init(
                stream,
                OpenDatabase(cache));
        }

        public static async Task<JMDict> CreateAsync(string path, string cache)
        {
            return await new JMDict().InitAsync(
                path,
                OpenDatabase(cache));
        }

        public static async Task<JMDict> CreateAsync(Stream stream, string cache)
        {
            return await new JMDict().InitAsync(
                stream,
                OpenDatabase(cache));
        }

        private static LiteDatabase OpenDatabase(string cachePath)
        {
            return new LiteDatabase(new FileDiskService(cachePath, journal: false));
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }

    internal class DbDictEntryKeyValue
    {
        public long Id { get; set; }

        public string LookupKey { get; set; }

        [BsonRef("entries")]
        public List<DbDictEntry> Values { get; set; }

        public KeyValuePair<string, IEnumerable<JMDictEntry>> To(Func<DbDictEntry, JMDictEntry> entryMapper)
        {
            return new KeyValuePair<string, IEnumerable<JMDictEntry>>(
                LookupKey,
                Values.Select(entryMapper));
        }

        public static DbDictEntryKeyValue From(KeyValuePair<string, IEnumerable<JMDictEntry>> kvp, Func<JMDictEntry, DbDictEntry> entryMapper, int id = 0)
        {
            return new DbDictEntryKeyValue
            {
                Id = id,
                LookupKey = kvp.Key,
                Values = kvp.Value.Select(entryMapper).ToList()
            };
        }
    }

    internal class DbDictEntry
    {
        public long Id { get; set; }

        public long SequenceNumber { get; set; }

        public List<string> Readings { get; set; }

        public List<string> Kanji { get; set; }

        [BsonRef("senses")]
        public List<DbSense> Senses { get; set; }

        public JMDictEntry To(Func<DbSense, JMDictSense> senseMapper)
        {
            return new JMDictEntry(
                SequenceNumber,
                Readings,
                Kanji,
                Senses.Select(senseMapper));
        }

        public static DbDictEntry From(JMDictEntry entry, Func<JMDictSense, DbSense> senseMapper, int id = 0)
        {
            return new DbDictEntry
            {
                Id = id,
                Kanji = entry.Kanji.ToList(),
                Readings = entry.Readings.ToList(),
                SequenceNumber = entry.SequenceNumber,
                Senses = entry.Senses.Select(senseMapper).ToList()
            };
        }
    }

    internal class DbSense
    {
        public long Id { get; set; }

        public EdictPartOfSpeech? POS { get; set; }

        public string PartOfSpeech { get; set; }

        public string Description { get; set; }

        public JMDictSense To()
        {
            return new JMDictSense(
                POS?.Some() ?? Option.None<EdictPartOfSpeech>(),
                PartOfSpeech,
                Description);
        }

        public static DbSense From(JMDictSense sense, int id = 0)
        {
            return new DbSense
            {
                Id = id,
                Description = sense.Description,
                PartOfSpeech = sense.PartOfSpeech,
                POS = sense.Type.ToNullable()
            };
        }
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
            IEnumerable<string> readings,
            IEnumerable<string> kanji,
            IEnumerable<JMDictSense> senses)
        {
            SequenceNumber = sequenceNumber;
            Readings = readings.ToList();
            Kanji = kanji.ToList();
            Senses = senses.ToList();
        }
    }

    public class JMDictSense
    {
        public Option<EdictPartOfSpeech> Type { get; }

        public string PartOfSpeech { get; }

        public string Description { get; }

        public JMDictSense(Option<EdictPartOfSpeech> type, string pos, string text)
        {
            Type = type;
            PartOfSpeech = pos;
            Description = text;
        }

        public override string ToString()
        {
            return PartOfSpeech + "\n" + Description;
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

        [Description("Ichidan verb - zuru verb - (alternative form of -jiru verbs)")]
        vz = 21,

        [Description("Kuru verb - special class")]
        vk = 22,

        [Description("irregular nu verb")]
        vn = 23,

        [Description("noun or participle which takes the aux. verb suru")]
        vs = 24,

        [Description("su verb - precursor to the modern suru")]
        vs_c = 25,

        [Description("suru verb - irregular")]
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

    enum EdictDialect
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
