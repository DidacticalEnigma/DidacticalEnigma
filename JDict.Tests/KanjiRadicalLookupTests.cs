using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using NUnit.Framework;
using Utility.Utils;

namespace JDict.Tests
{
    [TestFixture]
    class KanjiRadicalLookupTests
    {
        [Test]
        public void Test()
        {
            KanjiRadicalLookup lookup;
            using (var reader = File.OpenText(TestDataPaths.Radkfile))
            {
                lookup = new KanjiRadicalLookup(Radkfile.Parse(reader));
            }


            CollectionAssert.AreEquivalent(
                "籥鑰龠瀹爚禴籲鸙龡龢龣龥".AsCodePoints().Select(CodePoint.FromInt),
                lookup.SelectRadical(new[]{ CodePoint.FromString("龠")}));
            CollectionAssert.AreEquivalent(
                "龥籲".AsCodePoints().Select(CodePoint.FromInt),
            lookup.SelectRadical(new[]{CodePoint.FromString("龠"), CodePoint.FromString("ハ")}));
            // 龠瀹爚禴龡龢籥鑰龣龥鸙籲
            // 籥鑰龠瀹爚禴籲鸙龡龢龣龥
        }
    }
}
