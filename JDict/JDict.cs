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

namespace JDict
{

    // represents a lookup over an JMdict file
    public class JMDict : IDisposable
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(JdicRoot));

        private Dictionary<string, List<JMDictEntry>> root;

        private JMDict Init(Stream stream)
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
                root = new Dictionary<string, List<JMDictEntry>>();
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
                    if(!root.ContainsKey(kvp.Key))
                        root[kvp.Key] = new List<JMDictEntry>();
                    root[kvp.Key].Add(new JMDictEntry(
                        xmlEntry.Number,
                        xmlEntry.ReadingElements.Select(r => r.Reb),
                        xmlEntry.KanjiElements?.Select(k => k.Key) ?? Enumerable.Empty<string>(),
                        CreateSense(xmlEntry)));
                }
            }
            return this;
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
                    EdictTypeUtils.FromDescriptionOrNull(partOfSpeech?.FirstOrDefault(pos => EdictTypeUtils.FromDescriptionOrNull(pos) != null) ?? ""),
                    partOfSpeechString,
                    string.Join("/", s.Glosses.Select(g => g.Text.Trim()))));
                previousPartOfSpeech = partOfSpeech;
            }

            return sense;
        }

        private async Task<JMDict> InitAsync(Stream stream)
        {
            // TODO: not a lazy way
            await Task.Run(() => Init(stream));
            return this;
        }

        public IEnumerable<JMDictEntry> Lookup(string v)
        {
            root.TryGetValue(v, out var entry);
            return entry;
        }

        public IEnumerable<(JMDictEntry entry, string match)> PartialWordLookup(string v)
        {
            var regex = new Regex("^" + Regex.Escape(v).Replace(@"/\\", ".") + "$");
            return WordLookupByPredicate(word => regex.IsMatch(word));
        }

        public IEnumerable<(JMDictEntry entry, string match)> WordLookupByPredicate(Func<string, bool> matcher)
        {
            return root
                .Where(kvp => matcher(kvp.Key))
                .SelectMany(kvp => kvp.Value.Select(entry => (entry, kvp.Key)));
        }

        private async Task<JMDict> InitAsync(string path)
        {
            using(var file = File.OpenRead(path))
            {
                return await InitAsync(file);
            }
        }

        private JMDict Init(string path)
        {
            using (var file = File.OpenRead(path))
            {
                return Init(file);
            }
        }

        private JMDict()
        {

        }

        public static JMDict Create(string path)
        {
            return new JMDict().Init(path);
        }

        public static JMDict Create(Stream stream)
        {
            return new JMDict().Init(stream);
        }

        public static async Task<JMDict> CreateAsync(string path)
        {
            return await new JMDict().InitAsync(path);
        }

        public static async Task<JMDict> CreateAsync(Stream stream)
        {
            return await new JMDict().InitAsync(stream);
        }

        public void Dispose()
        {

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
        public EdictType? Type { get; }

        public string PartOfSpeech { get; }

        public string Description { get; }

        public JMDictSense(EdictType? type, string pos, string text)
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
        public static EdictType FromDescription(string description)
        {
            return mapping[description];
        }

        public static EdictType? FromDescriptionOrNull(string description)
        {
            if(mapping.TryGetValue(description, out var value))
            {
                return value;
            }

            return null;
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
                    false);

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

        [Description("kuru verb - special class")]
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
        adj_pn
    }
}
