using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using JetBrains.dotMemoryUnit;
using NUnit.Framework;

namespace JDict.Tests
{
    class PerformanceTests
    {
        private KanjiDict kanjiDict;
        private JDict.Kradfile kradfile;
        private Radkfile radkfile;
        private KanjiProperties kanjiProperties;
        private CodePoint[] radicalCodePoint;
        private IKanjiOrdering ordering;

        public PerformanceTests()
        {
            kanjiDict = JDict.KanjiDict.Create(TestDataPaths.KanjiDic);
            kradfile = new JDict.Kradfile(TestDataPaths.Kradfile, Encoding.UTF8);
            radkfile = new Radkfile(TestDataPaths.Radkfile, Encoding.UTF8);
            kanjiProperties = new KanjiProperties(kanjiDict, kradfile, radkfile, new RadicalRemapper(kradfile, radkfile));
            radicalCodePoint = radicalCodePoint = new[]{CodePoint.FromString("一") };
            ordering = kanjiProperties.KanjiOrderings.First();
        }

        [Explicit]
        [Test]
        public void Performance()
        {
            var cache = Path.Combine(Path.GetTempPath(), "whatever");
            File.Delete(cache);
            try
            {
                //var baseline = dotMemory.Check();
                var watch = Stopwatch.StartNew();

                using (var jnedict = JMDict.Create(TestDataPaths.JMDict, cache))
                {
                    var elapsed = watch.Elapsed;
                    /*var _ = dotMemory.Check(m =>
                    {
                        var traffic = m.GetTrafficFrom(baseline);
                        Assert.Less(traffic.AllocatedMemory.SizeInBytes, 4096L * 1024 * 1024);
                    });*/
                    //Assert.Less(elapsed, TimeSpan.FromSeconds(20));
                }
            }
            finally
            {
                File.Delete(cache);
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
    }
}
