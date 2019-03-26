using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using NUnit.Framework;

namespace JDict.Tests
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
    }
}
