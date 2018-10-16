using System.Collections.Generic;
using NMeCab;

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