using System;
using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Models.LanguageService;
using JDict;
using NMeCab;
using Optional;

namespace DidacticalEnigma.Core.Utils
{
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