using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.dotMemoryUnit;
using NUnit.Framework;

namespace JDict.Tests
{
    class PerformanceTests
    {
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
    }
}
