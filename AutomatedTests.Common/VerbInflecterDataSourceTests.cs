using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.LanguageService;
using JDict;
using NUnit.Framework;
using Optional;

namespace AutomatedTests
{
    [TestFixture]
    class VerbInflecterDataSourceTests
    {
        private static VerbConjugationDataSource dataSource;

        private static JMDictLookup jmdict;

        [OneTimeSetUp]
        public void SetUp()
        {
            jmdict = JDict.JMDictLookup.Create(TestDataPaths.JMDict, TestDataPaths.JMDictCache);
            dataSource = new VerbConjugationDataSource(jmdict);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            jmdict.Dispose();
        }

        [Test]
        public async Task Test()
        {
            var result = await dataSource.Answer(new Request(
                "落",
                new WordInfo("落ち着ける"),
                "落ち着ける",
                () => "落ち着ける"),
                CancellationToken.None);
            var boolOpt = result.Map(x => x.Paragraphs.Any(p => p is TextParagraph t && t.Content.Any(a => a.Content.Contains("落ち着けて"))));
            Assert.AreEqual(true.Some(), boolOpt);
        }
    }
}
