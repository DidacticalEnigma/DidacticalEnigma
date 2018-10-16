using System;
using System.Collections.Generic;
using System.Text;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IMeCab<out TMeCabEntry> : IDisposable
        where TMeCabEntry : IMeCabEntry
    {
        IEnumerable<TMeCabEntry> ParseToEntries(string text);
    }
}
