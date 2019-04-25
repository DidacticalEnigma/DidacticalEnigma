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

    public static class MorphologicalAnalyzerExt
    {
        public static IEnumerable<IEnumerable<WordInfo>> BreakIntoSentences<TEntry>(this IMorphologicalAnalyzer<TEntry> morphologicalAnalyzer, string input)
            where TEntry : IEntry
        {
            if (input.Trim() == "")
                return Enumerable.Empty<IEnumerable<WordInfo>>();
            return input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Select(line =>
                {
                    return morphologicalAnalyzer.ParseToEntries(line)
                        .Where(a => a.IsRegular)
                        .Select(word => new WordInfo(
                            word.SurfaceForm,
                            word.PartOfSpeech,
                            word.DictionaryForm,
                            word.PartOfSpeechInfo.Contains(PartOfSpeechInfo.Pronoun) ? Option.Some(EdictPartOfSpeech.pn) : word.Type,
                            word.Reading));
                });
        }
    }
}