using System;
using System.Collections.Generic;
using System.Linq;
using JDict;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class IpadicEntry : IEntry
    {
        public bool IsRegular { get; }

        public IpadicEntry(string surfaceForm, Option<string> feature)
        {
            SurfaceForm = surfaceForm;
            IsRegular = feature.HasValue;
            if(!IsRegular)
                return;
            var features = feature
                .Map(f => f.Split(','))
                .ValueOr(Array.Empty<string>());
            PartOfSpeech = MeCabEntryParser.PartOfSpeechFromString(MeCabEntryParser.OrNull(features, 0));
            ConjugatedForm = MeCabEntryParser.OrNull(features, 4);
            Type = MeCabEntryParser.TypeFromString(ConjugatedForm);
            Inflection = MeCabEntryParser.OrNull(features, 5);
            DictionaryForm = MeCabEntryParser.OrNull(features, 6);
            Reading = MeCabEntryParser.OrNull(features, 7);
            Pronunciation = MeCabEntryParser.OrNull(features, 8);
            PartOfSpeechSections = features
                .Take(4)
                .Where(f => f != "*")
                .ToList()
                .AsReadOnly();
            IsIndependent = MeCabEntryParser.IsIndependentFromSections(PartOfSpeechSections);
        }

        public Option<EdictType> Type { get; }

        public string SurfaceForm { get; }

        public PartOfSpeech PartOfSpeech { get; }

        public IEnumerable<string> PartOfSpeechSections { get; }

        public IEnumerable<PartOfSpeechInfo> PartOfSpeechInfo =>
            PartOfSpeechSections.Select(MeCabEntryParser.PartOfSpeechInfoFromString);

        public string ConjugatedForm { get; }

        public string Inflection { get; }

        public string Reading { get; }

        public string DictionaryForm { get; }

        public string Pronunciation { get; }

        public bool? IsIndependent { get; }
    }
}