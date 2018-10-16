using System;
using System.Collections.Generic;
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
                .Map(f => f.Split(','))
                .ValueOr(Array.Empty<string>());

            PartOfSpeech = MeCabEntryParser.PartOfSpeechFromString(MeCabEntryParser.OrNull(features, 0));
            NotInflected = MeCabEntryParser.OrNull(features, 7);
        }

        public string ConjugatedForm { get; }
        public string Inflection { get; }
        public bool? IsIndependent { get; }
        public bool IsRegular { get; }
        public string OriginalForm { get; }
        public PartOfSpeech PartOfSpeech { get; }
        public IEnumerable<PartOfSpeechInfo> PartOfSpeechInfo { get; }
        public IEnumerable<string> PartOfSpeechSections { get; }
        public string Pronunciation { get; }
        public string Reading { get; }
        public string NotInflected { get; }
        public Option<EdictType> Type { get; }
    }
}