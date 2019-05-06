using NMeCab;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DidacticalEnigma.Core.Models.LanguageService;
using JDict;

namespace AutomatedTests
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
                DicDir = TestDataPaths.Unidic,
                UseMemoryMappedFile = true
            };
            tagger = MeCabTagger.Create(mecabParam);
            mecabParam.LatticeLevel = MeCabLatticeLevel.Zero;
            mecabParam.OutputFormatType = "yomi";
            mecabParam.AllMorphs = false;
            mecabParam.Partial = true;
        }

        [Explicit]
        [Test]
        public void Tanaka()
        {
            var sentences = new Tanaka(TestDataPaths.Tanaka, Encoding.UTF8).AllSentences();
            var features = new HashSet<string>();
            var sentencesFiltered = new HashSet<string>();
            var n = 0;
            foreach(var rawSentence in sentences.Select(s => s.JapaneseSentence))
            {
                Console.WriteLine(tagger.Parse(rawSentence));
                var c = tagger.ParseToNodes(rawSentence);
                foreach (var morpheme in c)
                {
                    var feature = morpheme.Feature;
                    if(feature != null)
                        Console.WriteLine($"{morpheme.Surface} {feature}");
                    n++;
                    if (n == 20)
                        Assert.Fail();
                }
            }
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

    internal static class MeCabExt
    {
        public static IEnumerable<MeCabNode> ParseToNodes(this MeCabTagger tagger, string text)
        {
            for (var node = tagger.ParseToNode(text); node != null; node = node.Next)
            {
                yield return node;
            }
        }
    }
}
