using System;
using System.Collections.Generic;
using DidacticalEnigma.Core.Models.LanguageService;
using Utility.Utils;

namespace DidacticalEnigma.RestApi.InternalServices
{
    public class ParsedText
    {
        private List<KeyValuePair<int, (int outerIndex, int innerIndex)>> positionInformation;

        public ParsedText(string fullText, IReadOnlyList<IReadOnlyList<WordInfo>> wordInformation)
        {
            this.FullText = fullText;
            WordInformation = wordInformation;

            var positionInformation = new List<KeyValuePair<int, (int outerIndex, int innerIndex)>>();
            int position = 0;
            foreach (var (outer, outerIndex) in wordInformation.Indexed())
            {
                foreach (var (inner, innerIndex) in outer.Indexed())
                {
                    positionInformation.Add(
                        KeyValuePair.Create(position, (outerIndex, innerIndex)));
                    position += inner.RawWord.Length;
                }
            }

            this.positionInformation = positionInformation;
        }

        public string FullText { get; }

        public IReadOnlyList<IReadOnlyList<Core.Models.LanguageService.WordInfo>> WordInformation { get; }

        public (int wordPosition, int outerIndex, int innerIndex) GetIndicesAtPosition(int position)
        {
            if(position < 0)
                throw new ArgumentException(nameof(position));

            var result = LowerBound(
                i => positionInformation[i],
                positionInformation.Count,
                position,
                kvp => kvp.Key,
                Comparer<int>.Default);
            if (result >= positionInformation.Count)
                result = positionInformation.Count - 1;
            if (position < positionInformation[result].Key)
                result--;
            return (positionInformation[result].Key, positionInformation[result].Value.outerIndex, positionInformation[result].Value.innerIndex);
        }

        private static int LowerBound<T, TKey>(Func<int, T> lookup, int len, TKey lookupKey, Func<T, TKey> selector, IComparer<TKey> comparer)
        {
            int left = 0;
            int right = len;
            while (left < right)
            {
                var m = GetMidpoint(left, right);
                var record = lookup(m);
                var recordKey = selector(record);
                switch (comparer.Compare(recordKey, lookupKey))
                {
                    case var x when x < 0:
                        left = m + 1;
                        break;
                    default:
                        right = m;
                        break;
                }
            }

            return left;
        }

        private static int GetMidpoint(int l, int r)
        {
            // To replace if you suffer overflows
            return (l + r) / 2;
        }
    }
}
