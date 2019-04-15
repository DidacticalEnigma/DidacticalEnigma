using System.Linq;
using JDict;
using NUnit.Framework;

namespace AutomatedTests
{
    [TestFixture]
    class JnedictTests
    {
        private static Jnedict jnedict;

        [OneTimeSetUp]
        public void SetUp()
        {
            jnedict = Jnedict.Create(TestDataPaths.JMnedict, TestDataPaths.JMnedictCache);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            jnedict.Dispose();
        }

        [Test]
        public void LookupBasic()
        {
            var entries = jnedict.Lookup("南") ?? Enumerable.Empty<JnedictEntry>();
            Assert.True(entries.Any(e =>
                e.Reading.Contains("みなみ") &&
                e.Translation.Any(t => t.Type.Contains(JnedictType.fem) && t.Translation.Contains("Minami"))));
        }
    }
}
