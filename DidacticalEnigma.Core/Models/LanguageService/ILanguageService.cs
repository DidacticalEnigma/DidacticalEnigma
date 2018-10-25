using System;
using System.Collections.Generic;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    [Obsolete]
    public interface ILanguageService : IDisposable
    {
        IEnumerable<CodePoint> LookupRelatedCharacters(CodePoint point);

        Option<IEnumerable<CodePoint>> LookupRadicals(Kanji kanji);

        IEnumerable<Radical> AllRadicals();

        string LookupRomaji(Kana kana);

        IEnumerable<CodePoint> LookupByRadicals(IEnumerable<CodePoint> radicals);

        CodePoint LookupCharacter(int codePoint);

        CodePoint LookupCharacter(string s, int position = 0);

        IEnumerable<IEnumerable<WordInfo>> BreakIntoSentences(string input);
    }



    // composite 
}
