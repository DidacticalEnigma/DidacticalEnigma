using System;
using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Utils;
using JDict;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class UnidicMeCabEntry : IMeCabEntry
    {
        public UnidicMeCabEntry(string originalForm, Option<string> feature)
        {
            OriginalForm = originalForm;
            IsRegular = feature.HasValue;
            if (!IsRegular)
                return;

            var features = feature
                .Map(f => StringExt.SplitWithQuotes(f, ',', '"').ToArray())
                .ValueOr(Array.Empty<string>());

            PartOfSpeech = MeCabEntryParser.PartOfSpeechFromString(MeCabEntryParser.OrNull(features, 0));
            ConjugatedForm = MeCabEntryParser.OrNull(features, 5);
            NotInflected = MeCabEntryParser.OrNull(features, 7);
            Type = MeCabEntryParser.TypeFromString(ConjugatedForm);
            PartOfSpeechSections = features
                .Skip(1)
                .Take(3)
                .Where(f => f != "*")
                .ToList()
                .AsReadOnly();
            IsIndependent = MeCabEntryParser.IsIndependentFromSections(PartOfSpeechSections);
        }

        public string ConjugatedForm { get; }
        public string Inflection { get; }
        public bool? IsIndependent { get; }
        public bool IsRegular { get; }
        public string OriginalForm { get; }
        public PartOfSpeech PartOfSpeech { get; }
        public IEnumerable<PartOfSpeechInfo> PartOfSpeechInfo =>
            PartOfSpeechSections.Select(MeCabEntryParser.PartOfSpeechInfoFromString);
        public IEnumerable<string> PartOfSpeechSections { get; }
        public string Pronunciation { get; }
        public string Reading { get; }
        public string NotInflected { get; }
        public Option<EdictType> Type { get; }
    }
}