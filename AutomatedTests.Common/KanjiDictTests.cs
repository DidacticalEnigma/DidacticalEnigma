using NUnit.Framework;
using JDict;

namespace AutomatedTests
{
    [TestFixture]
    class KanjiDictTests
    {
        [Test]
        public void Test()
        {
            var kanjiDict = KanjiDict.Create(TestDataPaths.KanjiDic);
            var entry = kanjiDict.Lookup("南");
            entry.Match(e =>
            {
                CollectionAssert.AreEqual(new[]{"みなみ"}, e.KunReadings);
                Assert.AreEqual(e.FrequencyRating, 341);
            }, () =>
            {
                Assert.Fail();
            });
        }

        [Test]
        public void Test2()
        {
            var kanjiDict = KanjiDict.Create(TestDataPaths.KanjiDic);
            var entry = kanjiDict.LookupCodePoint('南');
            entry.Match(e =>
            {
                CollectionAssert.AreEqual(new[] { "みなみ" }, e.KunReadings);
                Assert.AreEqual(e.FrequencyRating, 341);
            }, () =>
            {
                Assert.Fail();
            });
        }
    }
}
