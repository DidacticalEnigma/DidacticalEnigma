using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using NMeCab;
using NUnit.Framework;

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
            Assert.True(entries.Any(e => e.Kanji.Contains("鼻の下を伸ばす")));
        }
    }
}
