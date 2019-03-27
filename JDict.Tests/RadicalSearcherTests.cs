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
            searcher = new RadicalSearcher(lookup.AllRadicals);
        }

        private static TestCaseData[] basicTestCaseData = new TestCaseData[]
        {
            new TestCaseData("", Enumerable.Empty<RadicalSearcherResult>()),
            new TestCaseData("      \t     ", Enumerable.Empty<RadicalSearcherResult>()),
            new TestCaseData("  龠  ", new[]{ new RadicalSearcherResult(2, 1, "龠", CodePoint.FromInt('龠')) }),
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
