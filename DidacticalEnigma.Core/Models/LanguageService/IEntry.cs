using System;
using System.Collections.Generic;
using JDict;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IEntry
    {
        // surface form is the form that appears in the text "as is"
        // possibly inflected
        string SurfaceForm { get; }
        // pronunciation
        string Pronunciation { get; }
        // reading
        string Reading { get; }
        // dictionary form is the base form of a word, pre inflections
        // in English, given surface forms "going", "went", a dictionary form is "go"
        string DictionaryForm { get; }
        string ConjugatedForm { get; }
        string Inflection { get; }

        bool IsRegular { get; }
        [Obsolete]
        PartOfSpeech PartOfSpeech { get; }
        [Obsolete]
        IEnumerable<PartOfSpeechInfo> PartOfSpeechInfo { get; }
        [Obsolete]
        IEnumerable<string> PartOfSpeechSections { get; }
        [Obsolete]
        Option<EdictPartOfSpeech> Type { get; }
    }
}