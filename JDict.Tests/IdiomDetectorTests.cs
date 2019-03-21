using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using NMeCab;
using NUnit.Framework;
using Utility.Utils;

namespace JDict.Tests
{
    [TestFixture]
    class IdiomDetectorTests
    {
        private static JMDict jmdict;

        private static IMorphologicalAnalyzer<IpadicEntry> ipadicMecab;

        private static IdiomDetector idiomDetector;

        [OneTimeSetUp]
        public void SetUp()
        {
            jmdict = JDict.JMDict.Create(TestDataPaths.JMDict, TestDataPaths.JMDictCache);
            ipadicMecab = new MeCabIpadic(new MeCabParam
            {
                DicDir = TestDataPaths.Ipadic
            });
            idiomDetector = new IdiomDetector(jmdict, ipadicMecab, TestDataPaths.IdiomsCache);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            jmdict.Dispose();
            idiomDetector.Dispose();
        }

        [Test]
        public void Basic()
        {
            var entries = idiomDetector.Detect("鼻の下");
            var result = entries.First(e => e.DictionaryEntry.Kanji.Contains("鼻の下を伸ばす"));
            CollectionAssert.AreEqual(new[]
            {
                ("鼻の下", true),
                ("を伸ばす", false)
            }, result.RenderedHighlights);
        }

        [Test]
        public void Basic2()
        {
            var entries = idiomDetector.Detect("じゃねえかな");
            var result = entries.First(e => e.DictionaryEntry.Readings.Contains("じゃしょういちにょ"));
            CollectionAssert.AreEqual(new[]
            {
                ("じゃ", true),
                ("しょういちにょ", false)
            }, result.RenderedHighlights);
        }

        [Test]
        public void Basic3()
        {
            var entries = idiomDetector.Detect("明日").ToList();
            Assert.True(entries.DistinctBy(e => e.DictionaryEntry.SequenceNumber).Count() == entries.Count);
            Assert.AreEqual(3, entries.Count);
        }
    }
}
