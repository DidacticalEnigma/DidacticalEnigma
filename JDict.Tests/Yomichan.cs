﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JDict.Json;
using Newtonsoft.Json;
using NUnit.Framework;

namespace JDict.Tests
{
    [TestFixture]
    class Yomichan
    {
        private string serialized =
            @"[[""焚き木"",""たきぎ"",""n"","""",9,[""firewood"",""kindling"",""fuel""],1365010,""""],[""焚き木"",""たきぎ"",""n"","""",8,[""piece(s) of firewood""],1365010,""""]]";

        private List<YomichanDictionaryEntry> deserialized = new List<YomichanDictionaryEntry>
        {
            new YomichanDictionaryEntry
            {
                Expression = "焚き木",
                Reading = "たきぎ",
                DefinitionTags = "n",
                Rules = "",
                Score = 9,
                Glossary = new[] {"firewood", "kindling", "fuel"},
                Sequence = 1365010,
                TermTags = ""
            },
            new YomichanDictionaryEntry
            {
                Expression = "焚き木",
                Reading = "たきぎ",
                DefinitionTags = "n",
                Rules = "",
                Score = 8,
                Glossary = new[] {"piece(s) of firewood"},
                Sequence = 1365010,
                TermTags = ""
            }
        };

        [Test]
        public void Deserialization()
        {
            var actual = JsonConvert.DeserializeObject<List<YomichanDictionaryEntry>>(serialized);
            Assert.AreEqual(actual.Count, deserialized.Count);

            AssertEqual(actual[0], deserialized[0]);
            AssertEqual(actual[1], deserialized[1]);
        }

        [Test]
        public void Serialization()
        {
            var actual = JsonConvert.SerializeObject(deserialized);
            Assert.AreEqual(actual, serialized);
        }

        void AssertEqual(YomichanDictionaryEntry l, YomichanDictionaryEntry r)
        {
            Assert.AreEqual(l.Expression, r.Expression);
            Assert.AreEqual(l.DefinitionTags, r.DefinitionTags);
            Assert.AreEqual(l.Reading, r.Reading);
            Assert.AreEqual(l.Rules, r.Rules);
            Assert.AreEqual(l.Score, r.Score);
            Assert.AreEqual(l.Sequence, r.Sequence);
            Assert.AreEqual(l.TermTags, r.TermTags);
            CollectionAssert.AreEqual(l.Glossary, r.Glossary);
        }
    }
}