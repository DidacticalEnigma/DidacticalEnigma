using NMeCab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("JDict.Tests")]

namespace DidacticalEnigma.Utils
{
    public static class NMeCabExt
    {
        public static IEnumerable<MeCabNode> ParseToNodes(this MeCabTagger tagger, string text)
        {
            for (var node = tagger.ParseToNode(text); node != null; node = node.Next)
            {
                yield return node;
            }
        }

        public static IEnumerable<MeCabEntry> ParseToEntries(this MeCabTagger tagger, string text)
        {
            return tagger.ParseToNodes(text).Select(n => new MeCabEntry(n));
        }
    }
}
