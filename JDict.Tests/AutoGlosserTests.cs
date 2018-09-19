using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Core.Models.Project;
using NMeCab;
using NUnit.Framework;

namespace JDict.Tests
{
    [TestFixture]
    class AutoGlosserTests
    {
        private static AutoGlosser glosser;
        private JMDict jmdict;
        private LanguageService lang;

        [Test]
        public void Basic()
        {
            {
                var notes = glosser.Gloss("これは私");
                CollectionAssert.AreEqual(new[]
                {
                    new GlossNote("これ",
                        "this (indicating an item near the speaker, the action of the speaker, or the current topic)"),
                    new GlossNote("は", "Particle は - topic marker particle"),
                    new GlossNote("私", "I/me"),
                }, notes);
            }
            {
                var notes = glosser.Gloss("それは問題ね");
            }
            {
                var notes = glosser.Gloss("事前に男の歌声録音してたんだ");
                CollectionAssert.AreEqual(new[]
                {
                    new GlossNote("事前",
                        "prior/beforehand/in advance/before the fact/ex ante"),
                    new GlossNote("に", "Particle に - at (place, time)/in/on/during"),
                    new GlossNote("男", "man/male"),
                    new GlossNote("の", "Particle の - indicates possessive"),
                    new GlossNote("歌声", "singing voice/(sound of) singing"),
                    new GlossNote("録音", "(audio) recording"),
                    new GlossNote("し て た ん だ", "suru, to do/to carry out/to perform, verbalizing suffix + inflections")
                }, notes);
            }
            {
                var notes = glosser.Gloss("リボンが赤だったから\n同じ学年のはずだけど\nどのクラスの奴も\n知らないらしくて");
                var r = string.Join("\n", notes.Select(x => x.ToString()));
                CollectionAssert.AreEqual(new[]
                {
                    new GlossNote("リボン", "ribbon"),
                    new GlossNote("が", "Particle が - indicates sentence subject (occasionally object)"),
                    new GlossNote("赤", "red"),
                    new GlossNote("だった", "was"),
                    new GlossNote("から", "Particle から - from (e.g. time, place, numerical quantity)/since"),
                    new GlossNote("同じ", "same/identical/equal/uniform/equivalent/similar/common (origin)/changeless/alike"),
                    new GlossNote("学年", "year in school/grade in school"),
                    new GlossNote("の", "Particle の - indicates possessive"),
                    new GlossNote("はずだ", "be supposed or expected to (do)/be sure to (do)/ought to (do)"),
                    new GlossNote("けど", "but/however/although"),
                    new GlossNote("どの", "which/what (way)"),
                    new GlossNote("クラス", "class"),
                    new GlossNote("の", "Particle の - indicates possessive"),
                    new GlossNote("奴", "he/she/him/her"),
                    new GlossNote("も", "Particle も - too/also/in addition/as well/(not) either (in a negative sentence)"),
                    new GlossNote("知ら ない らしく", "to be aware of/to know/to be conscious of/to cognize/to cognise + inflections"),
                    new GlossNote("て", "Particle て - you said/he said/she said/they said"), 
                }, notes);
            }
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            var baseDir = Tagger.baseDir;
            var kanjidict = JDict.KanjiDict.Create(Path.Combine(baseDir, @"dic\kanjidic2.xml"));
            var kradfile = new JDict.Kradfile(Path.Combine(baseDir, @"dic\kradfile1_plus_2_utf8"), Encoding.UTF8);
            var radkfile = new Radkfile(Path.Combine(baseDir, @"dic\radkfile1_plus_2_utf8"), Encoding.UTF8);
            var kanaProperties = new KanaProperties(
                Path.Combine(baseDir, @"dic\hiragana_romaji.txt"),
                Path.Combine(baseDir, @"dic\katakana_romaji.txt"),
                Path.Combine(baseDir, @"dic\kana_related.txt"),
                Encoding.UTF8);
            this.lang = new LanguageService(
                new MeCab(new MeCabParam
                {
                    DicDir = Path.Combine(baseDir, @"dic\ipadic"),
                }),
                EasilyConfusedKana.FromFile(Path.Combine(baseDir, @"dic\confused.txt")),
                kradfile,
                radkfile,
                kanjidict,
                kanaProperties);
            this.jmdict = JDict.JMDict.Create(Path.Combine(baseDir, "dic", "JMdict_e"));
            glosser = new AutoGlosser(lang, jmdict);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            lang.Dispose();
            jmdict.Dispose();
        }
    }
}
