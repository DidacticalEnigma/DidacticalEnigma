using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AutomatedTests
{
    [TestFixture]
    class SandBox
    {
        [Explicit]
        [Test]
        public void Sandbox()
        {
            var l = Enumerable.Range(0, 1000000).ToList();
            var counter = 0;
            l.Sort(Comparer<int>.Create((left, right) =>
            {
                counter++;
                return Comparer<int>.Default.Compare(left, right);
            }));
            Console.WriteLine(counter);
        }
    }
}
