using DidacticalEnigma.Models;
using System;
using System.Collections.Generic;

namespace DidacticalEnigma
{
    public interface ILanguageService : IDisposable
    {
        IEnumerable<CodePoint> LookupRelatedCharacters(CodePoint point);

        IEnumerable<CodePoint> LookupRadicals(Kanji kanji);

        IEnumerable<CodePoint> AllRadicals();

        string LookupRomaji(Kana kana);

        IEnumerable<CodePoint> LookupByRadicals(IEnumerable<CodePoint> radicals);

        CodePoint LookupCharacter(int codePoint);

        CodePoint LookupCharacter(string s, int position = 0);

        WordInfo LookupWord(string word);

        IEnumerable<IEnumerable<WordInfo>> BreakIntoSentences(string input);
    }



    // composite 
}
