using System;
using System.Collections.Generic;
using System.Text;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IMeCab : IDisposable
    {
        IEnumerable<MeCabEntry> ParseToEntries(string text);
    }
}
