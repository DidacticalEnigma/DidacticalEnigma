using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                        var mainkanjiElement = e.KanjiElements?.FirstOrDefault();
                        return Enumerable.Repeat(mainkanjiElement, mainkanjiElement != null ? 1 : 0)
                            ?.Select(k => new KeyValuePair<string, JdicEntry>(k.Key, e))
                            ?.Concat(e.ReadingElements.Select(r => new KeyValuePair<string, JdicEntry>(r.Reb, e)))
                            ?? Enumerable.Empty<KeyValuePair<string, JdicEntry>>();
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
                        xmlEntry.Senses.Select(s => new JMDictSense(
                            string.Join("/", s.PartOfSpeech ?? Enumerable.Empty<string>()),
                            string.Join("/", s.Glosses.Select(g => g.Text.Trim()))))));
                }
            }
            return this;
        }

        public IEnumerable<JMDictEntry> Lookup(string v)
        {
            root.TryGetValue(v, out var entry);
            return entry;
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

        public void Dispose()
        {

        }
    }

    public class JMDictEntry
    {
        public long SequenceNumber { get; }

        public IEnumerable<string> Readings { get; }

        public IEnumerable<string> Kanji { get; }

        public IEnumerable<string> PartOfSpeech { get; }

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
        public string PartOfSpeech { get; }

        public string Description { get; }

        public JMDictSense(string pos, string text)
        {
            PartOfSpeech = pos;
            Description = text;
        }

        public override string ToString()
        {
            return PartOfSpeech + "\n" + Description;
        }
    }
}
