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

        private static readonly TestCaseData[] TestCases =
        {
            new TestCaseData("リボンが赤だったから\n同じ学年のはずだけど\nどのクラスの奴も\n知らないらしくて", new[]
            {
                new GlossNote("リボン", "ribbon"),
                new GlossNote("が", "Particle が - indicates sentence subject (occasionally object)"),
                new GlossNote("赤", "red/crimson/scarlet"),
                new GlossNote("だっ た", "was"),
                new GlossNote("から", "Particle から - from (e.g. time, place, numerical quantity)/since"),
                new GlossNote("同じ", "same/identical/equal/uniform/equivalent/similar/common (origin)/changeless/alike"),
                new GlossNote("学年", "academic year/school year"),
                new GlossNote("の", "Particle の - indicates possessive"),
                new GlossNote("はず だ", "be supposed or expected to (do)/be sure to (do)/ought to (do)"),
                new GlossNote("けど", "Particle けど - but/however/although"),
                new GlossNote("どの", "which/what (way)"),
                new GlossNote("クラス", "class"),
                new GlossNote("の", "Particle の - indicates possessive"),
                new GlossNote("奴", "he/she/him/her"),
                new GlossNote("も", "Particle も - too/also/in addition/as well/(not) either (in a negative sentence)"),
                new GlossNote("知ら ない", "unknown/strange"),
                new GlossNote("らしく", "seeming .../appearing ... + inflections"),
                new GlossNote("て", "Particle て - you said/he said/she said/they said"),
            }),
            new TestCaseData("これは私", new[]
            {
                new GlossNote("これ",
                    "this (indicating an item near the speaker, the action of the speaker, or the current topic)"),
                new GlossNote("は", "Particle は - topic marker particle"),
                new GlossNote("私", "I/me"),
            }),
            new TestCaseData("事前に男の歌声録音してたんだ", new[]
            {
                new GlossNote("事前",
                    "prior/beforehand/in advance/before the fact/ex ante"),
                new GlossNote("に", "Particle に - at (place, time)/in/on/during"),
                new GlossNote("男", "man/male"),
                new GlossNote("の", "Particle の - indicates possessive"),
                new GlossNote("歌声", "singing voice/(sound of) singing"),
                new GlossNote("録音", "(audio) recording"),
                new GlossNote("し て た ん だ", "suru, to do/to carry out/to perform, verbalizing suffix + inflections")
            }),
            new TestCaseData("理由を話して\nもらえるかしら?", new[]
            {
                new GlossNote("理由", "reason/pretext/motive"),
                new GlossNote("を", "Particle を - indicates direct object of action"),
                new GlossNote("話して", "to talk/to speak/to converse/to chat + inflections"),
                new GlossNote("もらえる", "could you (give me)"),
                new GlossNote("かしら", "Particle かしら - I wonder")
            }),
            new TestCaseData("強引な理由だけれど\nこれを納得させれば\nもう探されないはずよ", new[]
            {
                new GlossNote("強引", "overbearing/coercive/pushy/forcible/high-handed"),
                new GlossNote("な", ""),
                new GlossNote("理由", "reason/pretext/motive"),
                new GlossNote("だ", ""),
                new GlossNote("けれど", "Particle けれど - but/however/although"),
                new GlossNote("これ", "this person "),
                new GlossNote("を", "indicates subject of causative expression"),
                new GlossNote("納得させれば", "consent/assent/agreement/understanding/comprehension/grasp in causative (to cause/to force/to let) and conditional"),
                new GlossNote("もう", "already/yet/by now/(not) anymore"),
                new GlossNote("探さ れ", "to search (for something desired, needed)/to look for, in potential form"),
                new GlossNote("ない", "not"),
                new GlossNote("はず", "expectation that something took place, will take place or was in some state/it should be so/bound to be/expected to be/must be"),
                new GlossNote("よ", "Sentence ender that gives a “you know” feeling, often when relaying new information."),
            }),
            new TestCaseData("男の子が喜ぶのを\nわかってやってる\nんだろうけど", new[]
            {
                new GlossNote("男の子", "boy/male child/baby boy"),
                new GlossNote("が", "Particle が - indicates sentence subject (occasionally object)"),
                new GlossNote("喜ぶ", "to be delighted/to be glad/to be pleased"),
                new GlossNote("の", "nominalizer"),
                new GlossNote("を", "Particle を - indicates direct object of action"),
                new GlossNote("わかっ て", "to understand/to comprehend/to grasp/to see/to get/to follow in -te form"),
                new GlossNote("やっ", ""),
                new GlossNote("てる", "to be ...-ing/to have been ...-ing"),
                new GlossNote("ん", "short for の"),
                new GlossNote("だろ う", ""),
                new GlossNote("けど", "Particle けど - but/however/although"),
            }),
            new TestCaseData("手加減する", new[]
            {
                new GlossNote("手加減", "going easy on someone/making allowances/using discretion/taking various circumstances into account"),
                new GlossNote("する", "suru, to do/to carry out/to perform, verbalizing suffix"),
            }),
            new TestCaseData("貴様ら", new[]
            {
                new GlossNote("貴様", "you/you bastard/you son of a bitch"),
                new GlossNote("ら", "pluralizing suffix")
            }),
            // https://japanese.stackexchange.com/a/62312/31447
            new TestCaseData("何もかも手にした気でいたんだ", new[]
            {
                new GlossNote("何もかも", "anything and everything/just about everything"),
                new GlossNote("手 に し た", "to hold (in one's hand)/to take (into one's hand)/to own/to obtain + inflections"),
                new GlossNote("気 で い た", ""),
                new GlossNote("んだ", "")
            }), 
        };

        [TestCaseSource(nameof(TestCases))]
        public void Basic(string input, IEnumerable<GlossNote> expected)
        {
            var notes = glosser.Gloss(input).ToList();
            Console.WriteLine(string.Join("\n", notes.Select(x => x.ToString())));
            CollectionAssert.AreEqual(expected, notes);
            //var notes = glosser.Gloss("それは問題ね");
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            var kanjidict = JDict.KanjiDict.Create(TestDataPaths.KanjiDic);
            var kradfile = new JDict.Kradfile(TestDataPaths.Kradfile, Encoding.UTF8);
            var radkfile = new Radkfile(TestDataPaths.Radkfile, Encoding.UTF8);
            var kanaProperties = new KanaProperties2(
                TestDataPaths.Kana,
                Encoding.UTF8);
            this.lang = new LanguageService(
                new MeCabIpadic(new MeCabParam
                {
                    DicDir = TestDataPaths.Ipadic,
                }), 
                EasilyConfusedKana.FromFile(TestDataPaths.EasilyConfused),
                kradfile,
                radkfile,
                kanjidict,
                kanaProperties,
                new RadicalRemapper(kradfile, radkfile));
            this.jmdict = JDict.JMDict.Create(TestDataPaths.JMDict, TestDataPaths.JMDictCache);
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
