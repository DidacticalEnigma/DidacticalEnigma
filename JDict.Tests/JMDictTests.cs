using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

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
                var entries = jmdict.Lookup("みなみ") ?? Enumerable.Empty<JMDictEntry>();
                Assert.True(entries.Any(e => e.Senses.Any(s => s.Glosses.Contains("south"))));
            }
        }

        [Test]
        public void LookupKanji()
        {
            {
                var entries = jmdict.Lookup("南") ?? Enumerable.Empty<JMDictEntry>();
                Assert.True(entries.Any(e => e.Senses.Any(s => s.Glosses.Contains("south"))));
            }
        }

        [Test]
        public void LookupOther()
        {
            {
                var entries = jmdict.Lookup("私") ?? Enumerable.Empty<JMDictEntry>();
                Assert.False(entries.Any(e => e.Senses.Any(s => s.Glosses.Contains("south"))));
            }
        }
    }
}
