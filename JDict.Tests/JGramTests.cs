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
    class JGramTests
    {
        [Test]
        public void Basic()
        {
            JGram.Entry entry;
            using (var reader = File.OpenText(TestDataPaths.JGram))
            {
                entry = JGram.Parse(reader).First(e => e.Id == 225);
                Assert.AreEqual("に加えて", entry.Key);
                Assert.AreEqual("にくわえて", entry.Reading);
                Assert.AreEqual("nikuwaete", entry.Romaji);
                Assert.AreEqual("in addition to", entry.Translation);
                Assert.AreEqual("Well, in addition to that, he also paid for the food.", entry.Example);
            }
        }
    }
}
