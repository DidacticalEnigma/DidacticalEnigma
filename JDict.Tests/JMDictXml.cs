using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using JDict.Internal.XmlModels;
using JetBrains.dotMemoryUnit;
using Newtonsoft.Json;
using NUnit.Framework;

namespace JDict.Tests
{
    [TestFixture]
    class JMDictXml
    {
        private readonly XmlSerializer serializer = new XmlSerializer(typeof(JdicRoot));

        private readonly XmlSerializer ser = new XmlSerializer(typeof(JdicEntry));

        [Explicit]
        [Test]
        public void T()
        {
            IReadOnlyList<JdicEntry> first, second;

            using (var stream = File.OpenRead(TestDataPaths.JMDict))
            using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
            {
                Deser(gzip);
            }
            using (var stream = File.OpenRead(TestDataPaths.JMDict))
            using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
            {
                first = Deserialize(gzip).ToList();
            }
            using (var stream = File.OpenRead(TestDataPaths.JMDict))
            using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
            {
                second = DeserializeNewImpl(gzip).ToList();
            }
            CollectionAssert.AreEqual(first.Select(x => SerializeToString(x)), second.Select(x => SerializeToString(x)));
        }

        [DotMemoryUnit(CollectAllocations = true)]
        [Explicit]
        [Test]
        public void A()
        {
            Z(Deserialize);
        }

        [DotMemoryUnit(CollectAllocations = true)]
        [Explicit]
        [Test]
        public void B()
        {
            Z(DeserializeNewImpl);
        }

        [DotMemoryUnit(CollectAllocations = true)]
        [Explicit]
        [Test]
        public void C()
        {
            var previous = dotMemory.Check();

            var n = new List<int>(32768 * 1024);

            previous = dotMemory.Check(m =>
            {
                Assert.Greater(m.GetTrafficFrom(previous).AllocatedMemory.SizeInBytes, 32L * 1024 * 1024 * sizeof(int));
            });

            previous = dotMemory.Check(m =>
            {
                Assert.Less(m.GetTrafficFrom(previous).AllocatedMemory.SizeInBytes, 1024L * 1024);
            });
        }

        public void Z(Func<Stream, IEnumerable<JdicEntry>> factory)
        {
            //var previous = dotMemory.Check();
            int i = 0;
            using (var stream = File.OpenRead(TestDataPaths.JMDict))
            using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
            {
                foreach (var a in factory(gzip))
                {
                    if (i == 0)
                    {
                        /*previous = dotMemory.Check(m =>
                        {
                            Assert.Less(m.GetTrafficFrom(previous).AllocatedMemory.SizeInBytes, 16L * 1024L * 1024);
                        });*/
                    }
                    i = (i + 1) % 50000;
                }
            }
        }

        string SerializeToString(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private IEnumerable<JdicEntry> Deserialize(Stream stream)
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
                foreach (var entry in ((JdicRoot)serializer.Deserialize(xmlReader)).Entries)
                {
                    yield return entry;
                }
            }
        }

        private IEnumerable<JdicEntry> DeserializeNewImpl(Stream stream)
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
                                    var entry = (JdicEntry)ser.Deserialize(elementReader);
                                    foreach (var s in entry.Senses ?? Array.Empty<Sense>())
                                    {
                                        foreach (var gloss in s.Glosses ?? Array.Empty<Gloss>())
                                        {
                                            if (gloss.Lang == null)
                                            {
                                                gloss.Lang = "eng";
                                            }
                                        }

                                        foreach (var loanSource in s.LoanWordSource ?? Array.Empty<LoanSource>())
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


        private void Deser(Stream stream)
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
                int i = 0;
                while (xmlReader.Read())
                {
                    switch (xmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            Console.WriteLine("Start Element {0}", xmlReader.Name);
                            break;
                        case XmlNodeType.Text:
                            Console.WriteLine("Text Node: {0}",
                                xmlReader.Value);
                            break;
                        case XmlNodeType.EndElement:
                            Console.WriteLine("End Element {0}", xmlReader.Name);
                            break;
                        default:
                            Console.WriteLine("Other node {0} with value {1}",
                                xmlReader.NodeType, xmlReader.Value);
                            break;
                    }

                    ++i;
                    if (i == 1000)
                        break;
                }
            }
        }
    }
}
