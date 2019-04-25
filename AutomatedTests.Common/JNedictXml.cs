using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using JDict.Xml;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AutomatedTests
{
    class JNedictXml
    {
        private readonly XmlSerializer serializer = new XmlSerializer(typeof(JMNedictRoot));

        private readonly XmlSerializer ser = new XmlSerializer(typeof(NeEntry));

        [Explicit]
        [Test]
        public void T()
        {
            IReadOnlyList<NeEntry> first, second;
            using (var stream = File.OpenRead(TestDataPaths.JMnedict))
            using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
            {
                first = Deserialize(gzip).ToList();
            }
            using (var stream = File.OpenRead(TestDataPaths.JMnedict))
            using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
            {
                second = DeserializeNewImpl(gzip).ToList();
            }
            CollectionAssert.AreEqual(first.Select(x => SerializeToString(x)), second.Select(x => SerializeToString(x)));
        }

        [Explicit]
        [Test]
        public void A()
        {
            Z(Deserialize);
        }

        [Explicit]
        [Test]
        public void B()
        {
            Z(DeserializeNewImpl);
        }

        public void Z(Func<Stream, IEnumerable<NeEntry>> factory)
        {
            //var previous = dotMemory.Check();
            using (var stream = File.OpenRead(TestDataPaths.JMDict))
            using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
            {
                foreach (var a in factory(gzip))
                {

                }
            }
        }

        string SerializeToString(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private IEnumerable<NeEntry> Deserialize(Stream stream)
        {
            var xmlSettings = new XmlReaderSettings
            {
                CloseInput = false,
                DtdProcessing = DtdProcessing.Parse, // we have local entities
                XmlResolver = null, // we don't want to resolve against external entities
                MaxCharactersFromEntities = 128 * 1024 * 1024 / 2, // 128 MB
                MaxCharactersInDocument = 512 * 1024 * 1024 / 2 // 512 MB
            };
            using (var xmlReader = XmlReader.Create(stream, xmlSettings))
            {
                return ((JMNedictRoot)serializer.Deserialize(xmlReader)).Entries;
            }
        }

        private IEnumerable<NeEntry> DeserializeNewImpl(Stream stream)
        {
            var xmlSettings = new XmlReaderSettings
            {
                CloseInput = false,
                DtdProcessing = DtdProcessing.Parse, // we have local entities
                XmlResolver = null, // we don't want to resolve against external entities
                MaxCharactersFromEntities = 128 * 1024 * 1024 / 2, // 128 MB
                MaxCharactersInDocument = 512 * 1024 * 1024 / 2 // 512 MB
            };
            using (var xmlReader = XmlReader.Create(stream, xmlSettings))
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "JMnedict")
                    {
                        while (xmlReader.Read())
                        {
                            if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "entry")
                            {
                                using (
                                    var elementReader =
                                        new StringReader(xmlReader.ReadOuterXml()))
                                {
                                    var entry = (NeEntry)ser.Deserialize(elementReader);
                                    yield return entry;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
