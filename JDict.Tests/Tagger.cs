using NMeCab;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace JDict.Tests
{
    [TestFixture]
    public class Tagger
    {
        private MeCabTagger tagger;

        public static readonly string baseDir = @"D:\DidacticalEnigma\DidacticalEnigma\";

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

        [Explicit]
        [Test]
        public void Tanaka()
        {
            var tanaka = new Tanaka(Path.Combine(baseDir, @"dic\examples.utf"), Encoding.UTF8);
            var meCab = new MeCab(new MeCabParam
            {
                DicDir = Path.Combine(baseDir, @"dic\ipadic"),
            });
            var sentences = tanaka.AllSentences()
                .Select(s => s.JapaneseSentence)
                .Select(s => meCab.ParseToEntries(s).Where(e => e.IsRegular).ToList());
            var features = new HashSet<string>();
            foreach(var sentence in sentences)
            {
                foreach(var word in sentence)
                {
                    foreach (var s in word.PartOfSpeechSections)
                    {
                        features.Add(s);
                    }
                }
            }
            var ss = string.Join("\n", features);
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
