using NUnit.Framework;
using System;
using System.Linq;
using System.Text;

namespace JDict.Tests
{
    [TestFixture]
    class Kradfile
    {
        private JDict.Kradfile kradfile;
        private JDict.Radkfile radkfile;

        [SetUp]
        public void SetUp()
        {
            kradfile = new JDict.Kradfile(TestDataPaths.Kradfile, Encoding.UTF8);
            radkfile = new JDict.Radkfile(TestDataPaths.Radkfile, Encoding.UTF8);
        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public void Krad()
        {
            int min = int.MaxValue;
            int max = int.MinValue;
            foreach (var kanjiToRadical in kradfile.AllRadicals())
            {
                var radical = kanjiToRadical.Key;
                var count = kanjiToRadical.Value.Count();
                min = Math.Min(count, min);
                max = Math.Max(count, max);
            }
        }
    }
}
