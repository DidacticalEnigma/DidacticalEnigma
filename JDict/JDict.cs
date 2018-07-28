using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using JDict.Internal.XmlModels;

namespace JDict
{
    // represents a lookup over an JMdict file
    internal class JMDict
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(JdicRoot));

        private Dictionary<string, JdicEntry> root;

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
                root = ((JdicRoot)serializer.Deserialize(xmlReader)).Entries
                    .SelectMany(e => e.KanjiElements?.Select(k => new KeyValuePair<string, JdicEntry>(k.Key, e)) ?? Enumerable.Empty<KeyValuePair<string, JdicEntry>>())
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            return this;
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
    }
}
