using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace JDict.Tests
{
    [TestFixture]
    class ZipTests
    {
        [Test]
        public void Test()
        {
            using (var zip = new ZipFile(@"D:\a\xc\jmdict_english.zip"))
            {
                var l = zip.Files.ToList();
                using (var file = zip.OpenFile(l[0]))
                using (var reader = new StreamReader(file))
                {
                    var a = reader.ReadToEnd();
                    ;
                }
            }
        }
    }
}
