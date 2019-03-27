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

        private static TestCaseData[] BasicTestCaseData = new TestCaseData[]
        {
            new TestCaseData("", Enumerable.Empty<KeyValuePair<string, CodePoint>>()),
            new TestCaseData("      \t     ", Enumerable.Empty<KeyValuePair<string, CodePoint>>()),
        };

        [Test]
        [TestCaseSource(nameof(BasicTestCaseData))]
        public void Basic(string input, IEnumerable<KeyValuePair<string, CodePoint>> expected)
        {
            var actual = searcher.Search(input);
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
