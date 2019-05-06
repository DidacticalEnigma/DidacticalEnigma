using System;
using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Models.LanguageService;
using JDict;
using Optional;

namespace DidacticalEnigma.Core.Models
{
    public class SentenceParser : ISentenceParser
    {
        private IMorphologicalAnalyzer<IEntry> analyzer;
        private JMDictLookup lookup;

        public SentenceParser(IMorphologicalAnalyzer<IEntry> analyzer, JMDictLookup lookup)
        {
            this.analyzer = analyzer;
            this.lookup = lookup;
        }

        public IEnumerable<IEnumerable<WordInfo>> BreakIntoSentences(string input)
        {
            if (input.Trim() == "")
                return Enumerable.Empty<IEnumerable<WordInfo>>();
            return input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Select(line =>
                {
                    return analyzer.ParseToEntries(line)
                        .Where(a => a.IsRegular)
                        .Select(word => new WordInfo(
                            word.SurfaceForm,
                            word.PartOfSpeech,
                            word.DictionaryForm,
                            word.GetPartOfSpeechInfo().Contains(PartOfSpeechInfo.Pronoun) ? Option.Some(EdictPartOfSpeech.pn) : word.Type,
                            word.Reading ?? lookup.Lookup(word.DictionaryForm ?? word.SurfaceForm)?.FirstOrDefault()?.Readings.First()));
                });
        }
    }
}