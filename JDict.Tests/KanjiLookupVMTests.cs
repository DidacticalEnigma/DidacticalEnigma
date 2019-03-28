using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.ViewModels;
using NUnit.Framework;

namespace JDict.Tests
{
    [RequiresThread(ApartmentState.STA)]
    [TestFixture]
    class KanjiLookupVMTests
    {
        private static KanjiRadicalLookup lookup;
        private static KanjiDict kanjiDict;
        private static IKanjiProperties kanjiProperties;
        private static RadicalSearcher searcher;

        private KanjiRadicalLookupControlVM vm;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            kanjiDict = KanjiDict.Create(TestDataPaths.KanjiDic);
            using (var reader = File.OpenText(TestDataPaths.Radkfile))
            {
                lookup = new KanjiRadicalLookup(Radkfile.Parse(reader), kanjiDict);
            }
            kanjiProperties = new KanjiProperties(
                kanjiDict,
                new JDict.Kradfile(TestDataPaths.Kradfile, Encoding.UTF8),
                new Radkfile(TestDataPaths.Radkfile, Encoding.UTF8),
                null);
            searcher = new RadicalSearcher(
                lookup.AllRadicals,
                KanjiAliveJapaneseRadicalInformation.Parse(TestDataPaths.KanjiAliveRadicals),
                new RadkfileKanjiAliveCorrelator(TestDataPaths.RadkfileKanjiAliveRadicalInfoCorrelationData));

        }

        [SetUp]
        public void SetUp()
        {
            vm = new KanjiRadicalLookupControlVM(lookup, kanjiProperties, searcher, searcher.RadicalTextForm);
        }

        [Ignore("the test itself is broken")]
        [Test]
        public void Test()
        {
            var x = vm.Radicals.First(r => r.CodePoint == CodePoint.FromInt('个'));
            var y = vm.Radicals.First(r => r.CodePoint == CodePoint.FromInt('ハ'));
            var z = vm.Radicals.First(r => r.CodePoint == CodePoint.FromInt('乃'));
            x.Selected = true;
            y.Selected = true;
            Assert.False(z.Enabled);
        }
    }
}
