using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using JDict;
using NMeCab;
using NUnit.Framework;
using Utility.Utils;

namespace AutomatedTests
{
    [TestFixture]
    class IdiomDetectorTests
    {
        private static JMDictLookup jmdict;

        private static IMorphologicalAnalyzer<IpadicEntry> ipadicMecab;

        private static IdiomDetector idiomDetector;

        [OneTimeSetUp]
        public void SetUp()
        {
            jmdict = JDict.JMDictLookup.Create(TestDataPaths.JMDict, TestDataPaths.JMDictCache);
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
        }

        [Test]
        public void Basic4()
        {
            var entries = idiomDetector.Detect("同士").ToList();
            Assert.True(entries.SelectMany(e => e.DictionaryEntry.Kanji).Contains("女同士"));
        }

        [Test]
        public void Basic5()
        {
            var entry = idiomDetector.Detect("何も問題ない").First(e => e.DictionaryEntry.Kanji.Contains("何の変哲もない"));
            CollectionAssert.AreEqual(new[]
            {
                ("何", true),
                ("の変哲もない", false)
            }, entry.RenderedHighlights);
        }
    }
}
