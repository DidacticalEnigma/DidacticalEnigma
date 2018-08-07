using DidacticalEnigma.Utils;
using NMeCab;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DidacticalEnigma.Models.LanguageService;

namespace JDict.Tests
{
    [TestFixture]
    public class Tagger
    {
        private MeCabTagger tagger;

        public static readonly string baseDir = @"C:\Users\IEUser\Desktop\DidacticalEnigma\DidacticalEnigma\";

        [SetUp]
        public void SetUp()
        {
            MeCabParam mecabParam = new MeCabParam
            {
                DicDir = Path.Combine(baseDir, @"dic\ipadic"),
            };
            tagger = MeCabTagger.Create(mecabParam);
            mecabParam.LatticeLevel = MeCabLatticeLevel.Zero;
            mecabParam.OutputFormatType = "lattice";
            mecabParam.AllMorphs = false;
            mecabParam.Partial = true;
        }

        [TearDown]
        public void TearDown()
        {
            tagger.Dispose();
        }

        [Ignore("takes too long to run")]
        [Test]
        public void Tanaka()
        {
            var tanaka = new Tanaka(Path.Combine(baseDir, @"dic\examples.utf"), Encoding.UTF8);
            var sentences = tanaka.AllSentences()
                .Select(s => s.JapaneseSentence)
                .Select(s => tagger.ParseToEntries(s).ToList());
            var partsOfSpeech = new HashSet<string>();
            foreach (var sentence in sentences)
            {
                Assert.AreEqual(sentence.First().Stat, MeCabNodeStat.Bos);
                Assert.AreEqual(sentence.Last().Stat, MeCabNodeStat.Eos);
                foreach (var word in sentence)
                {
                    
                }
            }
            var ss = string.Join("\n", partsOfSpeech);
            ;
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
