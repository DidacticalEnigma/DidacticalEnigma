using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using NMeCab;
using NUnit.Framework;

namespace JDict.Tests
{
    [TestFixture]
    class Romaji
    {
        private static IMeCab<IMeCabEntry> mecab;

        private static KanaProperties kanaProperties;

        private static readonly TestCaseData[] TestCases =
        {
            new TestCaseData("kore wa watashi", "これは私"), 
            new TestCaseData("shounen", "少年"), 
            new TestCaseData("shoujo", "少女"),
            new TestCaseData("fushigi", "ふしぎ"),
            new TestCaseData("fushigi", "不思議"),
            new TestCaseData("senpai", "先輩"),
            new TestCaseData("jun'ichirou", "じゅんいちろう"), 
        };

        [TestCaseSource(nameof(TestCases))]
        public void Test(string expected, string input)
        {
            var romaji = new ModifiedHepburn(mecab, kanaProperties);
            Assert.AreEqual(expected, romaji.ToRomaji(input));
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            var baseDir = TestDataPaths.BaseDir;
            mecab = new MeCabIpadic(new MeCabParam
            {
                DicDir = TestDataPaths.Ipadic,
            });
            kanaProperties = new KanaProperties(
                TestDataPaths.Hiragana,
                TestDataPaths.Katakana,
                TestDataPaths.HiraganaKatakana,
                TestDataPaths.KanaRelated,
                Encoding.UTF8);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            mecab.Dispose();
        }
    }
}
