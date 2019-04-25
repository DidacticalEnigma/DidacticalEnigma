﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Core.Models.Project;
using JDict;
using NMeCab;
using NUnit.Framework;

namespace AutomatedTests
{
    [TestFixture]
    class AutoGlosserTests
    {
        private static IAutoGlosser glosser;
        private JMDictLookup jmdict;
        private IMorphologicalAnalyzer<IEntry> mecab;

        private static readonly TestCaseData[] TestCases =
        {
            new TestCaseData("リボンが赤だったから\n同じ学年のはずだけど\nどのクラスの奴も\n知らないらしくて", new[]
            {
                new AutoGlosserNote("リボン", new[]{"ribbon"}),
                new AutoGlosserNote("が", new[]{"Particle が - indicates sentence subject (occasionally object)"}),
                new AutoGlosserNote("赤", new[]{"red/crimson/scarlet"}),
                new AutoGlosserNote("だっ た", new[]{"was"}),
                new AutoGlosserNote("から", new[]{"Particle から - from (e.g. time, place, numerical quantity)/since"}),
                new AutoGlosserNote("同じ", new[]{"same/identical/equal/uniform/equivalent/similar/common (origin)/changeless/alike"}),
                new AutoGlosserNote("学年", new[]{"academic year/school year"}),
                new AutoGlosserNote("の", new[]{"Particle の - indicates possessive"}),
                new AutoGlosserNote("はず だ", new[]{"be supposed or expected to (do)/be sure to (do)/ought to (do)"}),
                new AutoGlosserNote("けど", new[]{"Particle けど - but/however/although"}),
                new AutoGlosserNote("どの", new[]{"which/what (way)"}),
                new AutoGlosserNote("クラス", new[]{"class"}),
                new AutoGlosserNote("の", new[]{"Particle の - indicates possessive"}),
                new AutoGlosserNote("奴", new[]{"he/she/him/her"}),
                new AutoGlosserNote("も", new[]{"Particle も - too/also/in addition/as well/(not) either (in a negative sentence)"}),
                new AutoGlosserNote("知ら ない", new[]{"unknown/strange"}),
                new AutoGlosserNote("らしく", new[]{"seeming .../appearing ... + inflections"}),
                new AutoGlosserNote("て", new[]{"Particle て - you said/he said/she said/they said"}),
            }),
            new TestCaseData("これは私", new[]
            {
                new AutoGlosserNote("これ",
                    new[]{"this (indicating an item near the speaker, the action of the speaker, or the current topic)"}),
                new AutoGlosserNote("は", new[]{"Particle は - topic marker particle"}),
                new AutoGlosserNote("私", new[]{"I/me"}),
            }),
            new TestCaseData("今日はありがとう", new[]
            {
                new AutoGlosserNote("今日", new[]{"today/this day"}),
                new AutoGlosserNote("は", new[]{"Particle は - topic marker particle"}),
                new AutoGlosserNote("ありがとう", new[]{"Thank you/Thanks"}),
            }),
            new TestCaseData("かける言葉が見付からない", new[]
            {
                new AutoGlosserNote("かける", new[]{"to hang up (e.g. a picture on the wall)/to let hang/to suspend (from)/to hoist (e.g. sail)/to raise (e.g. flag)"}),
                new AutoGlosserNote("言葉 が 見付から ない", new[]{"at a loss for words/stumped for words"}),
            }),
            new TestCaseData("それは問題ね", new[]
            {
                new AutoGlosserNote("それ", new[]{""}),
                new AutoGlosserNote("は", new[]{""}),
                new AutoGlosserNote("問題", new[]{""}),
                new AutoGlosserNote("ね", new[]{""}),
            }),
            new TestCaseData("事前に男の歌声録音してたんだ", new[]
            {
                new AutoGlosserNote("事前",
                    new []{"prior/beforehand/in advance/before the fact/ex ante"}),
                new AutoGlosserNote("に", new[]{"Particle に - at (place, time)/in/on/during"}),
                new AutoGlosserNote("男", new[]{"man/male"}),
                new AutoGlosserNote("の", new[]{"Particle の - indicates possessive"}),
                new AutoGlosserNote("歌声", new[]{"singing voice/(sound of) singing"}),
                new AutoGlosserNote("録音", new[]{"(audio) recording"}),
                new AutoGlosserNote("し て た ん だ", new[]{"suru, to do/to carry out/to perform, verbalizing suffix + inflections"})
            }),
            new TestCaseData("理由を話して\nもらえるかしら?", new[]
            {
                new AutoGlosserNote("理由", new[]{"reason/pretext/motive"}),
                new AutoGlosserNote("を", new[]{"Particle を - indicates direct object of action"}),
                new AutoGlosserNote("話して", new[]{"to talk/to speak/to converse/to chat + inflections"}),
                new AutoGlosserNote("もらえる", new[]{"could you (give me)"}),
                new AutoGlosserNote("かしら", new []{"Particle かしら - I wonder"})
            }),
            new TestCaseData("強引な理由だけれど\nこれを納得させれば\nもう探されないはずよ", new[]
            {
                new AutoGlosserNote("強引", new[]{"overbearing/coercive/pushy/forcible/high-handed"}),
                new AutoGlosserNote("な", new[]{""}),
                new AutoGlosserNote("理由", new[]{"reason/pretext/motive"}),
                new AutoGlosserNote("だ", new[]{""}),
                new AutoGlosserNote("けれど", new[]{"Particle けれど - but/however/although"}),
                new AutoGlosserNote("これ", new[]{"this person "}),
                new AutoGlosserNote("を", new[]{"indicates subject of causative expression"}),
                new AutoGlosserNote("納得させれば", new[]{"consent/assent/agreement/understanding/comprehension/grasp in causative (to cause/to force/to let) and conditional"}),
                new AutoGlosserNote("もう", new[]{"already/yet/by now/(not) anymore"}),
                new AutoGlosserNote("探さ れ", new[]{"to search (for something desired, needed)/to look for, in potential form"}),
                new AutoGlosserNote("ない", new[]{"not"}),
                new AutoGlosserNote("はず", new[]{"expectation that something took place, will take place or was in some state/it should be so/bound to be/expected to be/must be"}),
                new AutoGlosserNote("よ", new[]{"Sentence ender that gives a “you know” feeling, often when relaying new information."}),
            }),
            new TestCaseData("男の子が喜ぶのを\nわかってやってる\nんだろうけど", new[]
            {
                new AutoGlosserNote("男の子", new[]{"boy/male child/baby boy"}),
                new AutoGlosserNote("が", new[]{"Particle が - indicates sentence subject (occasionally object)"}),
                new AutoGlosserNote("喜ぶ", new[]{"to be delighted/to be glad/to be pleased"}),
                new AutoGlosserNote("の", new[]{"nominalizer"}),
                new AutoGlosserNote("を", new[]{"Particle を - indicates direct object of action"}),
                new AutoGlosserNote("わかっ て", new[]{"to understand/to comprehend/to grasp/to see/to get/to follow in -te form"}),
                new AutoGlosserNote("やっ", new[]{""}),
                new AutoGlosserNote("てる", new[]{"to be ...-ing/to have been ...-ing"}),
                new AutoGlosserNote("ん", new[]{"short for の"}),
                new AutoGlosserNote("だろ う", new[]{""}),
                new AutoGlosserNote("けど", new[]{"Particle けど - but/however/although"}),
            }),
            new TestCaseData("手加減する", new[]
            {
                new AutoGlosserNote("手加減", new[]{"going easy on someone/making allowances/using discretion/taking various circumstances into account"}),
                new AutoGlosserNote("する", new[]{"suru, to do/to carry out/to perform, verbalizing suffix"}),
            }),
            new TestCaseData("貴様ら", new[]
            {
                new AutoGlosserNote("貴様", new[]{"you/you bastard/you son of a bitch"}),
                new AutoGlosserNote("ら", new []{ "pluralizing suffix" })
            }),
            // https://japanese.stackexchange.com/a/62312/31447
            new TestCaseData("何もかも手にした気でいたんだ", new[]
            {
                new AutoGlosserNote("何もかも", new[]{"anything and everything/just about everything"}),
                new AutoGlosserNote("手 に し た", new[]{"to hold (in one's hand)/to take (into one's hand)/to own/to obtain + inflections"}),
                new AutoGlosserNote("気 で い た", new[]{""}),
                new AutoGlosserNote("ん だ", new []{ "" })
            }),
            new TestCaseData("募集できないアイテムです", new[]
            {
                new AutoGlosserNote("募集", new[]{"anything and everything/just about everything"}),
                new AutoGlosserNote("でき", new[]{"to hold (in one's hand)/to take (into one's hand)/to own/to obtain + inflections"}),
                new AutoGlosserNote("ない", new[]{""}),
                new AutoGlosserNote("アイテム", new[]{""}),
                new AutoGlosserNote("です", new []{""})
            }),
            new TestCaseData("セーラー服", new[]
            {
                new AutoGlosserNote("セーラー服", new[]{"sailor suit/middy uniform"}),
            }),
            new TestCaseData("それもそうか", new[]
            {
                new AutoGlosserNote("それ も", new[]{"and in addition to that/even so"}),
                new AutoGlosserNote("そう か", new[]{"is that so? (generally rhetorical)"}),
            })
        };

        [TestCaseSource(nameof(TestCases))]
        public void DoesntCrash(string input, IEnumerable<AutoGlosserNote> expected)
        {
            var notes = glosser.Gloss(input).ToList();
        }

        [Explicit("see commit 8a9a4be79bcf20ffdb48721696839e6a6c7ac2c2")]
        [TestCaseSource(nameof(TestCases))]
        public void LengthsAreEqual(string input, IEnumerable<AutoGlosserNote> expected)
        {
            var notes = glosser.Gloss(input).ToList();
            Assert.AreEqual(expected.Count(), notes.Count);
        }

        [Explicit("see commit 8a9a4be79bcf20ffdb48721696839e6a6c7ac2c2")]
        [TestCaseSource(nameof(TestCases))]
        public void ContentsAreEqual(string input, IEnumerable<AutoGlosserNote> expected)
        {
            var notes = glosser.Gloss(input).ToList();
            Console.WriteLine(string.Join("\n",
                notes.Select(gloss =>
                    $"- {gloss.Foreign}:\n{string.Join("\n", gloss.GlossCandidates.Select(c => $"    - {c}"))}")));
            CollectionAssert.AreEqual(expected, notes);
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
            this.mecab = new MeCabIpadic(new MeCabParam
            {
                DicDir = TestDataPaths.Ipadic,
            });
            this.jmdict = JDict.JMDictLookup.Create(TestDataPaths.JMDict, TestDataPaths.JMDictCache);
            glosser = new AutoGlosserNext(mecab, jmdict, kanaProperties);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            mecab.Dispose();
            jmdict.Dispose();
        }
    }
}
