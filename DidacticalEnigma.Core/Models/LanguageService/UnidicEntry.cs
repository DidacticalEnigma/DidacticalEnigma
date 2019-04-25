using System;
using System.Collections.Generic;
using System.Linq;
using JDict;
using Optional;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class UnidicEntry : IEntry
    {
        public UnidicEntry(string surfaceForm, Option<string> feature)
        {
            SurfaceForm = surfaceForm;
            IsRegular = feature.HasValue;
            if (!IsRegular)
                return;
            
            var features = feature
                .Map(f => StringExt.SplitWithQuotes(f, ',', '"').ToArray())
                .ValueOr(Array.Empty<string>());

            PartOfSpeech = MeCabEntryParser.PartOfSpeechFromString(MeCabEntryParser.OrNull(features, 0));
            ConjugatedForm = MeCabEntryParser.OrNull(features, 5);
            DictionaryForm = MeCabEntryParser.OrNull(features,　10);
            Pronunciation = MeCabEntryParser.OrNull(features, 9);
            Reading = MeCabEntryParser.OrNull(features, 17);
            Type = MeCabEntryParser.TypeFromString(ConjugatedForm);
            PartOfSpeechSections = features
                .Skip(1)
                .Take(3)
                .Where(f => f != "*")
                .ToList()
                .AsReadOnly();
        }

        public string ConjugatedForm { get; }
        public string Inflection { get; }
        public bool IsRegular { get; }
        public string SurfaceForm { get; }
        public PartOfSpeech PartOfSpeech { get; }
        public IEnumerable<PartOfSpeechInfo> PartOfSpeechInfo =>
            PartOfSpeechSections.Select(MeCabEntryParser.PartOfSpeechInfoFromString);
        public IEnumerable<string> PartOfSpeechSections { get; }
        public string Pronunciation { get; }
        public string Reading { get; }
        public string DictionaryForm { get; }
        public Option<EdictPartOfSpeech> Type { get; }
    }
}