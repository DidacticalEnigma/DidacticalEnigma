using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Optional;

namespace JDict.Tests
{
    [TestFixture]
    class JMDictTests
    {
        private static JMDict jmdict;

        [OneTimeSetUp]
        public void SetUp()
        {
            jmdict = JDict.JMDict.Create(TestDataPaths.JMDict, TestDataPaths.JMDictCache);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            jmdict.Dispose();
        }

        [Test]
        public void LookupKana()
        {
            {
                var entries = jmdict.Lookup("みなみ");
                Assert.True(entries.Any(e => e.Senses.Any(s => s.Glosses.Contains("south"))));
            }
        }

        [Test]
        public void LookupKanji()
        {
            {
                var entries = jmdict.Lookup("南");
                Assert.True(entries.Any(e => e.Senses.Any(s => s.Glosses.Contains("south"))));
            }
        }

        [Test]
        public void LookupOther()
        {
            {
                var entries = jmdict.Lookup("私");
                Assert.False(entries.Any(e => e.Senses.Any(s => s.Glosses.Contains("south"))));
            }
        }

        [Test]
        public void LookupKangaeru()
        {
            {
                var entries = jmdict.Lookup("考える");
                Assert.True(entries.Any(e => e.Senses.Any(s => s.Type == EdictPartOfSpeech.v1.Some())));
            }
        }
    }
}
