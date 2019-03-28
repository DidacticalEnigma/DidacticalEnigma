using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DidacticalEnigma;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.ViewModels;
using NUnit.Framework;

namespace JDict.Tests
{
    [TestFixture]
    class PerformanceTests
    {
        private KanjiDict kanjiDict;
        private JDict.Kradfile kradfile;
        private Radkfile radkfile;
        private KanjiProperties kanjiProperties;
        private CodePoint[] radicalCodePoint;
        private IKanjiOrdering ordering;
        private RadicalSearcher radicalSearcher;
        private KanjiRadicalLookup lookup;

        public PerformanceTests()
        {
            kanjiDict = JDict.KanjiDict.Create(TestDataPaths.KanjiDic);
            kradfile = new JDict.Kradfile(TestDataPaths.Kradfile, Encoding.UTF8);
            radkfile = new Radkfile(TestDataPaths.Radkfile, Encoding.UTF8);
            kanjiProperties = new KanjiProperties(kanjiDict, kradfile, radkfile, new RadicalRemapper(kradfile, radkfile));
            radicalCodePoint = radicalCodePoint = new[]{CodePoint.FromString("一") };
            ordering = kanjiProperties.KanjiOrderings.First();
            radicalSearcher = new RadicalSearcher(
                lookup.AllRadicals,
                KanjiAliveJapaneseRadicalInformation.Parse(TestDataPaths.KanjiAliveRadicals),
                new RadkfileKanjiAliveCorrelator(TestDataPaths.RadkfileKanjiAliveRadicalInfoCorrelationData));
            using (var reader = File.OpenText(TestDataPaths.Radkfile))
            {
                lookup = new KanjiRadicalLookup(Radkfile.Parse(reader), kanjiDict);
            }
        }

        [Explicit]
        [Test]
        public void JmdictAllEntriesPerformance()
        {
            using (var jmdict = JMDict.Create(TestDataPaths.JMDict, TestDataPaths.JMDictCache))
            {
                var watch = Stopwatch.StartNew();
                var list = jmdict.AllEntries().ToList();
                var elapsed = watch.Elapsed;
                Assert.Less(elapsed, TimeSpan.FromSeconds(4));
            }
        }

        [Explicit]
        [Test]
        public void JMDictCacheCreationPerformance()
        {
            var cache = Path.Combine(Path.GetTempPath(), "whatever");
            File.Delete(cache);
            try
            {
                //var baseline = dotMemory.Check();
                var watch = Stopwatch.StartNew();

                using (var jdict = JMDict.Create(TestDataPaths.JMDict, cache))
                {
                    var elapsed = watch.Elapsed;
                    /*var _ = dotMemory.Check(m =>
                    {
                        var traffic = m.GetTrafficFrom(baseline);
                        Assert.Less(traffic.AllocatedMemory.SizeInBytes, 4096L * 1024 * 1024);
                    });*/
                    Assert.Less(elapsed, TimeSpan.FromSeconds(20));
                }
            }
            finally
            {
                File.Delete(cache);
            }
        }

        [Explicit]
        [Test]
        public void PartialLookupCreation()
        {
            using (var jmdict = JMDict.Create(TestDataPaths.JMDict, TestDataPaths.JMDictCache))
            {
                var watch = Stopwatch.StartNew();
                var lookup = new PartialWordLookup(jmdict, radicalSearcher, this.lookup);
                var elapsed = watch.Elapsed;
                Assert.Less(elapsed, TimeSpan.FromSeconds(1));
            }
        }

        [Explicit]
        [Test]
        public void RadicalLookup()
        {
            var watch = Stopwatch.StartNew();
            _ = kanjiProperties.LookupKanjiByRadicals(radicalCodePoint, ordering).ToList();
            Assert.Less(watch.Elapsed, TimeSpan.FromMilliseconds(1));
        }

        [Explicit]
        [Test]
        public void RadicalLookupNew()
        {
            var watch = Stopwatch.StartNew();
            _ = lookup.SelectRadical(radicalCodePoint);
            Assert.Less(watch.Elapsed, TimeSpan.FromMilliseconds(1));
        }
    }

    [RequiresThread(ApartmentState.STA)]
    public class ProgramStartupPerformanceTests
    {
        [Explicit]
        [Test]
        public void StartupTimeTest()
        {
            // cold boot
            using (var kernel = App.Configure(TestDataPaths.BaseDir))
            {
                kernel.BindFactory<ITextInsertCommand>(() => new MockInsertTextCommand());
                _ = kernel.Get<MainWindowVM>();
            }

            var watch = Stopwatch.StartNew();
            // hot boot
            using (var kernel = App.Configure(TestDataPaths.BaseDir))
            {
                kernel.BindFactory<ITextInsertCommand>(() => new MockInsertTextCommand());
                _ = kernel.Get<MainWindowVM>();
                watch.Stop();
            }

            Assert.Less(watch.Elapsed, TimeSpan.FromSeconds(10));
        }

        private class MockInsertTextCommand : ITextInsertCommand
        {
            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                
            }

            public event EventHandler CanExecuteChanged;
        }
    }
}
