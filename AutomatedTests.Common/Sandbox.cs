using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using JDict.Xml;
using NUnit.Framework;

namespace AutomatedTests
{
    [TestFixture]
    class SandBox
    {
        [Explicit]
        [Test]
        public void Sandbox()
        {
            var serializer = new XmlSerializer(typeof(JdicEntry));
            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter))
            {
                serializer.Serialize(xmlWriter, new JdicEntry(){ KanjiElements = new KanjiElement[0]});
                Console.WriteLine(stringWriter.ToString());
            }
        }
    }
}
