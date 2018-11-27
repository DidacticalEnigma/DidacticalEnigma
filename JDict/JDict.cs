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

        private static readonly int Version = 3;

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
                var entries = ((JdicRoot)serializer.Deserialize(xmlReader)).Entries
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

        public EdictType? Type { get; set; }

        public string PartOfSpeech { get; set; }

        public string Description { get; set; }

        public JMDictSense To()
        {
            return new JMDictSense(
                Type?.Some() ?? Option.None<EdictType>(),
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
                Type = sense.Type.ToNullable()
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
        public Option<EdictType> Type { get; }

        public string PartOfSpeech { get; }

        public string Description { get; }

        public JMDictSense(Option<EdictType> type, string pos, string text)
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
        public static Option<EdictType> FromDescription(string description)
        {
            return mapping.FromDescription(description);
        }

        public static bool IsVerb(this EdictType type)
        {
            return (int)type < (int)EdictType.n;
        }

        public static bool IsNoun(this EdictType type)
        {
            return (int)type < 256 && (int)type >= (int)EdictType.n;
        }

        private static EnumMapper<EdictType> mapping = new EnumMapper<EdictType>();
    }

    // Make sure to synchronize these constants with the values at LibJpConjSharp project
    public enum EdictType
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

        // Ichidan verb - kureru special class

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
    }
}
