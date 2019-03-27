using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class RadkfileKanjiAliveCorrelator : IReadOnlyDictionary<int, int>
    {
        private readonly IReadOnlyDictionary<int, int> underlying;

        public IEnumerator<KeyValuePair<int, int>> GetEnumerator()
        {
            return underlying.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) underlying).GetEnumerator();
        }

        public int Count => underlying.Count;

        public bool ContainsKey(int key)
        {
            return underlying.ContainsKey(key);
        }

        public bool TryGetValue(int key, out int value)
        {
            return underlying.TryGetValue(key, out value);
        }

        public int this[int key] => underlying[key];

        public IEnumerable<int> Keys => underlying.Keys;

        public IEnumerable<int> Values => underlying.Values;

        public RadkfileKanjiAliveCorrelator(string path)
        {
            underlying = File.ReadLines(path)
                .Select(line => line.Split(':'))
                .Where(components => components[0] != "x" && components[1] != "x")
                .ToDictionary(
                    components => char.ConvertToUtf32(components[0], 0),
                    components => char.ConvertToUtf32(components[1], 0));
        }
    }
}
