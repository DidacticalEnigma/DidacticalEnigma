using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Models.LanguageService;
using JDict;
using NUnit.Framework;

namespace JDict.Tests
{
    [TestFixture]
    class RadicalSearcherTests
    {
        private static KanjiDict kanjiDict;
        private static KanjiRadicalLookup lookup;
        private static RadicalSearcher searcher;

        [OneTimeSetUp]
        public void SetUp()
        {
            kanjiDict = KanjiDict.Create(TestDataPaths.KanjiDic);
            lookup = new KanjiRadicalLookup(Radkfile.Parse(TestDataPaths.Radkfile), kanjiDict);
            var entries = KanjiAliveJapaneseRadicalInformation.Parse(TestDataPaths.KanjiAliveRadicals);
            var remapper = new RadkfileKanjiAliveCorrelator(TestDataPaths.RadkfileKanjiAliveRadicalInfoCorrelationData);
            searcher = new RadicalSearcher(lookup.AllRadicals, entries, remapper);
        }

        private static TestCaseData[] basicTestCaseData = new TestCaseData[]
        {
            new TestCaseData("", Enumerable.Empty<RadicalSearcherResult>()),
            new TestCaseData("      \t     ", Enumerable.Empty<RadicalSearcherResult>()),
            new TestCaseData("  龠  ", new[]{ new RadicalSearcherResult(2, 1, "龠", CodePoint.FromInt('龠')) }),
            new TestCaseData("  龠ハ  ", new[]{ new RadicalSearcherResult(2, 1, "龠", CodePoint.FromInt('龠')), new RadicalSearcherResult(3, 1, "ハ", CodePoint.FromInt('ハ')) }),
            new TestCaseData("  龠;ハ  ", new[]{ new RadicalSearcherResult(2, 1, "龠", CodePoint.FromInt('龠')), new RadicalSearcherResult(4, 1, "ハ", CodePoint.FromInt('ハ')) }),
            new TestCaseData("  龠; ;ハ  ", new[]{ new RadicalSearcherResult(2, 1, "龠", CodePoint.FromInt('龠')), new RadicalSearcherResult(6, 1, "ハ", CodePoint.FromInt('ハ')) }),
            new TestCaseData("heart ハ  ", new[]{ new RadicalSearcherResult(0, 5, "heart", CodePoint.FromInt('心')), new RadicalSearcherResult(7, 1, "ハ", CodePoint.FromInt('ハ')) }),
        };

        [Test]
        [TestCaseSource(nameof(basicTestCaseData))]
        public void Basic(string input, IEnumerable<RadicalSearcherResult> expected)
        {
            var actual = searcher.Search(input);
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
