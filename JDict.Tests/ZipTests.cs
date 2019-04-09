using System.IO;
using System.Linq;
using NUnit.Framework;
using Utility.Utils;

namespace JDict.Tests
{
    [TestFixture]
    class ZipTests
    {
        [DependentOnKenkyuusha]
        [Test]
        public void Test()
        {
            using (var zip = new ZipFile(TestDataPaths.Kenkyusha5))
            {
                var l = zip.Files.ToList();
                using (var file = zip.OpenFile(l[0]))
                using (var reader = new StreamReader(file))
                {
                    _ = reader.ReadToEnd();
                }
            }
        }
    }
}
