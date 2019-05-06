using System.Collections.Generic;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface ISentenceParser
    {
        IEnumerable<IEnumerable<WordInfo>> BreakIntoSentences(string input);
    }
}