using System;
using System.Collections.Generic;
using System.Text;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IMorphologicalAnalyzer<out TEntry> : IDisposable
        where TEntry : IEntry
    {
        IEnumerable<TEntry> ParseToEntries(string text);
    }
}
