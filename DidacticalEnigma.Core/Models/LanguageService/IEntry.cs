using System;
using System.Collections.Generic;
using JDict;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IEntry
    {
        string OriginalForm { get; }
        string Pronunciation { get; }
        string Reading { get; }
        string NotInflected { get; }
        string ConjugatedForm { get; }
        string Inflection { get; }
        [Obsolete]
        bool? IsIndependent { get; }
        bool IsRegular { get; }
        PartOfSpeech PartOfSpeech { get; }
        IEnumerable<PartOfSpeechInfo> PartOfSpeechInfo { get; }
        IEnumerable<string> PartOfSpeechSections { get; }
        
        Option<EdictType> Type { get; }
    }
}