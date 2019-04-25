using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Models.LanguageService;
using JDict;
using NMeCab;
using NUnit.Framework;
using Optional;
using Utility.Utils;

namespace AutomatedTests
{
    [TestFixture]
    class MeCab
    {
        private static readonly TestCaseData[] Test =
        {
            new TestCaseData("これは俺", new[]
            {
                new DummyEntry
                {
                    SurfaceForm = "これ",
                    DictionaryForm = "これ",
                    Pronunciation = "コレ",
                    Reading = "コレ"
                },
                new DummyEntry
                {
                    SurfaceForm = "は",
                    DictionaryForm = "は",
                    Pronunciation = "ワ",
                    Reading = "ハ"
                },
                new DummyEntry
                {
                    SurfaceForm = "俺",
                    DictionaryForm = "俺",
                    Pronunciation = "オレ",
                    Reading = "オレ"
                }
            }),
            new TestCaseData("楽しかった", new[]
            {
                new DummyEntry
                {
                    SurfaceForm = "楽しかっ",
                    DictionaryForm = "楽しい",
                    Pronunciation = "タノシカッ",
                    Reading = "タノシカッ"
                },
                new DummyEntry
                {
                    SurfaceForm = "た",
                    DictionaryForm = "た",
                    Pronunciation = "タ",
                    Reading = "タ"
                }
            }), 
        };

        [TestCaseSource(nameof(Test))]
        public void BasicCompatibility(string sentence, IEnumerable<IEntry> expectedEntries)
        {
            var ipadicEntries = ipadicMecab.ParseToEntries(sentence).Where(e => e.IsRegular);
            // this is to make test cases fail in case the number of expecteds is less than the number of actuals
            var nullDummyEntry = new DummyEntry();
            foreach (var (i, e) in ipadicEntries.Zip(expectedEntries.Concat(Utility.Utils.EnumerableExt.Repeat(nullDummyEntry))))
            {
                //Assert.AreEqual(e.ConjugatedForm, i.ConjugatedForm);
                //Assert.AreEqual(e.Inflection, i.Inflection);
                Assert.AreEqual(e.SurfaceForm, i.SurfaceForm);
                //Assert.AreEqual(e.PartOfSpeechString, i.PartOfSpeechString);
                Assert.AreEqual(e.Pronunciation, i.Pronunciation);
                Assert.AreEqual(e.Reading, i.Reading);
                Assert.AreEqual(e.DictionaryForm, i.DictionaryForm);
            }
        }

        private static IMorphologicalAnalyzer<IEntry> ipadicMecab;

        [OneTimeSetUp]
        public void SetUp()
        {
            ipadicMecab = new MeCabIpadic(new MeCabParam
            {
                DicDir = TestDataPaths.Ipadic
            });
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            ipadicMecab.Dispose();
        }
    }

    public class DummyEntry : IEntry
    {
        private IEnumerable<PartOfSpeechInfo> partOfSpeechInfo;
        public string ConjugatedForm { get; set; }
        public string Inflection { get; set; }
        public bool? IsIndependent { get; set; }
        public bool IsRegular { get; set; } = true;
        public string SurfaceForm { get; set; }
        public PartOfSpeech PartOfSpeech { get; set; }

        public IEnumerable<PartOfSpeechInfo> PartOfSpeechInfo
        {
            set { partOfSpeechInfo = value; }
        }

        public IEnumerable<PartOfSpeechInfo> GetPartOfSpeechInfo()
        {
            return partOfSpeechInfo;
        }

        public IEnumerable<string> PartOfSpeechSections { get; set; }
        public string Pronunciation { get; set; }
        public string Reading { get; set; }
        public string DictionaryForm { get; set; }
        public Option<EdictPartOfSpeech> Type { get; set; }
    }
}
