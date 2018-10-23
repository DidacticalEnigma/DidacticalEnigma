using NMeCab;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DidacticalEnigma.Core.Models.LanguageService;

namespace JDict.Tests
{
    [TestFixture]
    public class Tagger
    {
        private MeCabTagger tagger;


        [SetUp]
        public void SetUp()
        {
            MeCabParam mecabParam = new MeCabParam
            {
                DicDir = TestDataPaths.Ipadic,
            };
            tagger = MeCabTagger.Create(mecabParam);
            mecabParam.LatticeLevel = MeCabLatticeLevel.Zero;
            mecabParam.OutputFormatType = "lattice";
            mecabParam.AllMorphs = false;
            mecabParam.Partial = true;
        }

        [Explicit]
        [Test]
        public void Tanaka()
        {
            var tanaka = new Tanaka(TestDataPaths.Tanaka, Encoding.UTF8);
            var meCab = new MeCabUnidic(new MeCabParam
            {
                DicDir = TestDataPaths.Unidic,
            });
            var sentences = tanaka.AllSentences();
            var features = new HashSet<string>();
            var sentencesFiltered = new HashSet<string>();
            foreach(var rawSentence in sentences.Select(s => s.JapaneseSentence))
            {
                var sentence = meCab.ParseToEntries(rawSentence)
                    .Where(e => e.IsRegular)
                    .ToList();
                foreach (var word in sentence)
                {
                    foreach (var s in word.PartOfSpeechSections)
                    {
                        var newElement = features.Add(s);
                        if (newElement)
                        {
                            sentencesFiltered.Add(rawSentence);
                        }
                    }
                }
            }
            var ss = string.Join("\n", sentencesFiltered);
            var xx = string.Join("\n", features);
            ;
        }

        [TearDown]
        public void TearDown()
        {
            tagger.Dispose();
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
