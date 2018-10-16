using System.Collections.Generic;
using System.Linq;
using NMeCab;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public abstract class MeCab<TMeCabEntry> : IMeCab<TMeCabEntry>
        where TMeCabEntry : IMeCabEntry
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

        public IEnumerable<TMeCabEntry> ParseToEntries(string text)
        {
            TMeCabEntry FromNode(MeCabNode node)
            {
                bool IsRegular(MeCabNode n) =>
                    !(n.Stat == MeCabNodeStat.Eos || n.Stat == MeCabNodeStat.Bos);

                return ToEntry(
                    node.Surface,
                    node.SomeWhen(IsRegular)
                        .Map(n => n.Feature));
            }

            return tagger.ParseToNodes(text).Select(FromNode);
        }

        protected abstract TMeCabEntry ToEntry(string surface, Option<string> features);

        public void Dispose()
        {
            tagger.Dispose();
        }
    }
}
