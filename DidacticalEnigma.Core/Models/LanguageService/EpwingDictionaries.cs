using System;
using System.Collections.Generic;
using JDict;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class EpwingDictionaries : IDisposable
    {
        private List<YomichanTermDictionary> dicts = new List<YomichanTermDictionary>();

        public IEnumerable<YomichanTermDictionary> Dictionaries => dicts;

        // Add method instead of constructor is provided by design
        public void Add(YomichanTermDictionary dict)
        {
            dicts.Add(dict);
        }

        public void Dispose()
        {
            foreach (var dict in dicts)
            {
                dict.Dispose();
            }
            dicts.Clear();
        }
    }
}
