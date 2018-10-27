using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using NUnit.Framework;

namespace JDict.Tests
{
    [TestFixture]
    class KanaPropertiesTests
    {
        public static readonly TestCaseData[] HiraganaConversion =
        {
            new TestCaseData("ドキドキ", "どきどき"), 
        };

        public static IKanaProperties kana;

        [OneTimeSetUp]
        public void SetUp()
        {
            kana = new KanaProperties2(TestDataPaths.Kana, Encoding.UTF8);
        }

        [TestCaseSource(nameof(HiraganaConversion))]
        public void ToHiragana(string input, string expected)
        {
            Assert.AreEqual(expected, kana.ToHiragana(input));
        }
    }
}
