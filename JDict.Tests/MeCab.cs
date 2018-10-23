using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using Microsoft.SqlServer.Server;
using NMeCab;
using NUnit.Framework;

namespace JDict.Tests
{
    [TestFixture]
    class MeCab
    {
        private static readonly TestCaseData[] Test =
        {
            new TestCaseData("これは私"),
            new TestCaseData("楽しかった"), 
        };

        [TestCaseSource(nameof(Test))]
        public void BasicCompatibility(string sentence)
        {
            var ipadicEntries = ipadicMecab.ParseToEntries(sentence).Where(e => e.IsRegular);
            var unidicEntries = unidicMecab.ParseToEntries(sentence).Where(e => e.IsRegular);
            foreach (var (i, u) in ipadicEntries.Zip(unidicEntries, (i, u) => (i, u)))
            {
                //Assert.AreEqual(i.ConjugatedForm, u.ConjugatedForm);
                Assert.AreEqual(i.Inflection, u.Inflection);
                Assert.AreEqual(i.OriginalForm, u.OriginalForm);
                //Assert.AreEqual(i.PartOfSpeech, u.PartOfSpeech);
                Assert.AreEqual(i.Pronunciation, u.Pronunciation);
                Assert.AreEqual(i.Reading, u.Reading);
                Assert.AreEqual(i.NotInflected, u.NotInflected);
            }
        }

        [Test]
        public void BasicProperties()
        {

        }

        private static IMorphologicalAnalyzer<IEntry> ipadicMecab;

        private static IMorphologicalAnalyzer<IEntry> unidicMecab;

        [OneTimeSetUp]
        public void SetUp()
        {
            ipadicMecab = new MeCabUnidic(new MeCabParam
            {
                DicDir = TestDataPaths.Ipadic
            });
            unidicMecab = new MeCabUnidic(new MeCabParam
            {
                DicDir = TestDataPaths.Unidic
            });
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            ipadicMecab.Dispose();
            unidicMecab.Dispose();
        }
    }
}
