using System.Collections.Generic;
using JDict;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IMeCabEntry
    {
        string ConjugatedForm { get; }
        string Inflection { get; }
        bool? IsIndependent { get; }
        bool IsRegular { get; }
        string OriginalForm { get; }
        PartOfSpeech PartOfSpeech { get; }
        IEnumerable<PartOfSpeechInfo> PartOfSpeechInfo { get; }
        IEnumerable<string> PartOfSpeechSections { get; }
        string Pronunciation { get; }
        string Reading { get; }
        Option<EdictType> Type { get; }
    }
}