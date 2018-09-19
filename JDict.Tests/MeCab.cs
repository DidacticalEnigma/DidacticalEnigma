using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using NMeCab;

namespace JDict.Tests
{
    public class MeCab : IMeCab
    {
        private readonly MeCabTagger tagger;

        public MeCab(MeCabParam mecabParam)
        {
            mecabParam.LatticeLevel = MeCabLatticeLevel.Zero;
            mecabParam.OutputFormatType = "wakati";
            mecabParam.AllMorphs = false;
            mecabParam.Partial = true;
            tagger = MeCabTagger.Create(mecabParam);
        }

        public IEnumerable<MeCabNode> ParseToNodes(MeCabTagger tagger, string text)
        {
            for(var node = tagger.ParseToNode(text); node != null; node = node.Next)
            {
                yield return node;
            }
        }

        public IEnumerable<MeCabEntry> ParseToEntries(string text)
        {
            MeCabEntry FromNode(MeCabNode n)
            {
                return new MeCabEntry(n.Surface, () => n.Feature, !(n.Stat == MeCabNodeStat.Eos || n.Stat == MeCabNodeStat.Bos));
            }

            return ParseToNodes(tagger, text).Select(FromNode);
        }

        public void Dispose()
        {
            tagger.Dispose();
        }
    }
}
