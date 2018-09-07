using System;
using System.Collections.Generic;
using System.Text;
using DidacticalEnigma.Utils;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IMeCab : IDisposable
    {
        IEnumerable<MeCabEntry> ParseToEntries(string text);
    }
}
