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
            var meCab = new MeCabUnidic(new MeCabParam
            {
                DicDir = TestDataPaths.Unidic,
            });
            var sentences = new Tanaka(TestDataPaths.Tanaka, Encoding.UTF8).AllSentences();
            var features = new HashSet<string>();
            var sentencesFiltered = new HashSet<string>();
            var count = 0;
            var sum = 0;
            var max = 0;
            var permutermSizeEstimate = 0;
            foreach(var rawSentence in sentences.Select(s => s.JapaneseSentence))
            {
                var c = meCab
                    .ParseToEntries(rawSentence)
                    .Count(e => e.IsRegular);
                count++;
                sum += c;
                max = Math.Max(c, max);
                permutermSizeEstimate += (Encoding.UTF8.GetByteCount(rawSentence)+1) * c;
            }
            Console.WriteLine($"Count: {count}");
            Console.WriteLine($"Max: {max}");
            Console.WriteLine($"Avg: {sum/count}");
            Console.WriteLine($"Permuterm size UTF-8: {permutermSizeEstimate / 1024 / 1024}MB");
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
