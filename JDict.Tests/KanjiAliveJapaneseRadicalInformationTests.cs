using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace JDict.Tests
{
    [TestFixture]
    class KanjiAliveJapaneseRadicalInformationTests
    {
        [Test]
        public void Basic()
        {
            var entries = KanjiAliveJapaneseRadicalInformation.Parse(TestDataPaths.KanjiAliveRadicals);
            var entry = entries.First(e => e.Literal == "𠆢");
            Assert.AreEqual(2, entry.StrokeCount);
            CollectionAssert.AreEqual(new[]{ "person" }, entry.Meanings);
        }
    }
}
