using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using FileMode = System.IO.FileMode;

namespace JDict
{
    // represents a lookup over an JMdict file
    public class JMDict : IDisposable
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(JdicRoot));

        private static readonly int Version = 1;

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

            senses.InsertBulk(sensesDict.Select(s => s.Value));
            entries.InsertBulk(entriesDict.Select(e => e.Value));
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

        public IEnumerable<(JMDictEntry entry, string match)> PartialWordLookup(string v)
        {
            var regex = new Regex("^" + Regex.Escape(v).Replace(@"/\\", ".") + "$");
            return WordLookupByPredicate(word => regex.IsMatch(word));
        }

        public IEnumerable<(JMDictEntry entry, string match)> WordLookupByPredicate(Func<string, bool> matcher)
        {
            var matches = kvps
                .FindAll()
                .Where(kvp => matcher(kvp.LookupKey))
                .Select(kvp => kvp.LookupKey)
                .ToList();

            return kvps
                .IncludeAll()
                .Find(kvp => matches.Contains(kvp.LookupKey))
                .SelectMany(kvp =>
                {
                    var mapping = SimpleMap(kvp);
                    return mapping.Value.Select(entry => (entry, mapping.Key));
                });
        }

        private KeyValuePair<string, IEnumerable<JMDictEntry>> SimpleMap(DbDictEntryKeyValue kvp)
        {
            return kvp.To(e => e.To(en => en.To()));
        }

        private async Task<JMDict> InitAsync(string path, LiteDatabase cache)
        {
            using (var file = File.OpenRead(path))
            {
                return await InitAsync(file, cache);
            }
        }

        private JMDict Init(string path, LiteDatabase cache)
        {
            using (var file = File.OpenRead(path))
            {
                return Init(file, cache);
            }
        }

        private JMDict()
        {

        }

        public static JMDict Create(string path, string cache)
        {
            return new JMDict().Init(
                path,
                OpenDatabase(File.Open(cache, FileMode.OpenOrCreate), dispose: true));
        }

        public static JMDict Create(Stream stream, Stream cache)
        {
            return new JMDict().Init(
                stream,
                OpenDatabase(cache, dispose: false));
        }

        public static async Task<JMDict> CreateAsync(string path, string cache)
        {
            return await new JMDict().InitAsync(
                path,
                OpenDatabase(File.Open(cache, FileMode.OpenOrCreate), dispose: true));
        }

        public static async Task<JMDict> CreateAsync(Stream stream, Stream cache)
        {
            return await new JMDict().InitAsync(
                stream,
                OpenDatabase(cache, dispose: false));
        }

        private static LiteDatabase OpenDatabase(Stream stream, bool dispose)
        {
            return new LiteDatabase(stream, disposeStream: dispose);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }

    internal class DbDictVersion : IEquatable<DbDictVersion>
    {
        public long Id { get; set; }

        public int DbVersion { get; set; }

        public long OriginalFileSize { get; set; }

        public byte[] OriginalFileHash { get; set; }

        public bool Equals(DbDictVersion other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return DbVersion.Equals(other.DbVersion) &&
                   OriginalFileSize == other.OriginalFileSize &&
                   OriginalFileHash?.SequenceEqual(other.OriginalFileHash ?? Enumerable.Empty<byte>()) == true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DbDictVersion) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = DbVersion.GetHashCode();
                hashCode = (hashCode * 397) ^ OriginalFileSize.GetHashCode();
                hashCode = (hashCode * 397) ^ (OriginalFileHash != null ? Hash(OriginalFileHash) : 0);
                return hashCode;
            }

            int Hash(byte[] h)
            {
                int x = 0;
                unchecked
                {   
                    foreach (var b in h)
                    {
                        x = x * 33 + b;
                    }
                }
                return x;
            }
        }

        public static bool operator ==(DbDictVersion left, DbDictVersion right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DbDictVersion left, DbDictVersion right)
        {
            return !Equals(left, right);
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
                Description,
                PartOfSpeech);
        }

        public static DbSense From(JMDictSense sense, int id = 0)
        {
            return new DbSense
            {
                Description = sense.Description,
                Id = id,
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
            return mapping.GetValueOrNone(description);
        }

        public static bool IsVerb(this EdictType type)
        {
            return (int)type < (int)EdictType.n;
        }

        public static bool IsNoun(this EdictType type)
        {
            return (int)type < 256 && (int)type >= (int)EdictType.n;
        }

        // https://stackoverflow.com/questions/2650080/how-to-get-c-sharp-enum-description-from-value
        private static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                    typeof(DescriptionAttribute),
                    inherit: false);

            if(attributes != null &&
               attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        private static readonly Dictionary<string, EdictType> mapping = Enum.GetValues(typeof(EdictType))
            .Cast<EdictType>()
            .ToDictionary(e => GetEnumDescription(e), e => e);
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
