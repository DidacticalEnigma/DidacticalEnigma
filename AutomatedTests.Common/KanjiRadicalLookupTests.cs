using System;
using System.IO;
using System.Linq;
using System.Unicode;
using DidacticalEnigma.Core.Models.LanguageService;
using JDict;
using NUnit.Framework;
using Utility.Utils;

namespace AutomatedTests
{
    [TestFixture]
    class KanjiRadicalLookupTests
    {
        private KanjiRadicalLookup lookup;
        private KanjiDict kanjiDict;

        [OneTimeSetUp]
        public void SetUp()
        {
            kanjiDict = KanjiDict.Create(TestDataPaths.KanjiDic);
            using (var reader = File.OpenText(TestDataPaths.Radkfile))
            {
                lookup = new KanjiRadicalLookup(Radkfile.Parse(reader), kanjiDict);
            }
        }

        [Explicit]
        [Test]
        public void Count()
        {
            var count = lookup.AllRadicals
                .Select(r => lookup.SelectRadical(Utility.Utils.EnumerableExt.OfSingle(r)))
                .Select(r => r.Kanji.Count)
                .Max();
            ;
        }

        [Explicit]
        [Test]
        public void RadicalUnicodeCategories()
        {
            foreach (var radical in lookup.AllRadicals)
            {
                Console.Write(radical.ToString());
                Console.Write(": ");
                var info = UnicodeInfo.GetCharInfo(radical.Utf32);
                Console.WriteLine(info.Name);
            }
        }

        [Test]
        public void Test()
        {
            CollectionAssert.AreEquivalent(
                "籥鑰龠瀹爚禴籲鸙龡龢龣龥".AsCodePoints().Select(CodePoint.FromInt),
                lookup.SelectRadical(new[]{ CodePoint.FromString("龠")})
                    .Kanji);
            Console.WriteLine($"Expected: {string.Join(",", "龥籲".AsCodePoints())}");
            Console.WriteLine($"Actual: {string.Join(",", lookup.SelectRadical(new[] { CodePoint.FromString("龠"), CodePoint.FromString("ハ") }).Kanji.Select(k => k.Utf32))}");
            CollectionAssert.AreEquivalent(
                "龥籲".AsCodePoints().Select(CodePoint.FromInt),
                lookup.SelectRadical(new[]{CodePoint.FromString("龠"), CodePoint.FromString("ハ")})
                .Kanji);
            CollectionAssert.AreEquivalent(
                "一｜亅个ハ冂口目冊竹貝頁龠廾".AsCodePoints().Select(CodePoint.FromInt),
                lookup.SelectRadical(new[] { CodePoint.FromString("龠"), CodePoint.FromString("ハ") })
                    .PossibleRadicals
                    .Where(r => r.Value)
                    .Select(r => r.Key));
            // 龠瀹爚禴龡龢籥鑰龣龥鸙籲
            // 籥鑰龠瀹爚禴籲鸙龡龢龣龥
        }
    }
}
