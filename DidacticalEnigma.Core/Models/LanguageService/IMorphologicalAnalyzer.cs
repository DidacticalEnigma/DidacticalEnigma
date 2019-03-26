using System;
using System.Collections.Generic;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IMorphologicalAnalyzer<out TEntry> : IDisposable
        where TEntry : IEntry
    {
        IEnumerable<TEntry> ParseToEntries(string text);
    }
}
