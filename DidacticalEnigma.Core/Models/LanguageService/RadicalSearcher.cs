using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Unicode;
using JDict;
using Optional;
using Optional.Collections;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class RadicalSearcherResult : IEquatable<RadicalSearcherResult>
    {
        public bool Equals(RadicalSearcherResult other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Start == other.Start && Length == other.Length && string.Equals(Text, other.Text);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RadicalSearcherResult) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Start;
                hashCode = (hashCode * 397) ^ Length;
                hashCode = (hashCode * 397) ^ Text.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(RadicalSearcherResult left, RadicalSearcherResult right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RadicalSearcherResult left, RadicalSearcherResult right)
        {
            return !Equals(left, right);
        }

        public int Start { get; }

        public int Length { get; }

        public string Text { get; }

        public CodePoint Radical { get; }

        public RadicalSearcherResult(int start, int length, string text, CodePoint radical)
        {
            Start = start;
            Length = length;
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Radical = radical ?? throw new ArgumentNullException(nameof(radical));
        }

        public override string ToString()
        {
            return $"{nameof(Start)}: {Start}, {nameof(Length)}: {Length}, {nameof(Text)}: {Text}, {nameof(Radical)}: {Radical}";
        }
    }

    public interface IRadicalSearcher
    {
        IReadOnlyList<RadicalSearcherResult> Search(string text);
    }

    public class RadicalSearcher : IRadicalSearcher
    {
        private readonly Dictionary<string, CodePoint> names;

        private readonly Dictionary<int, CodePoint> lookup;

        public IReadOnlyList<RadicalSearcherResult> Search(string text)
        {
            var entries = Split(text);
            return entries
                .Select(Match)
                .Values()
                .ToList();
        }

        public Option<RadicalSearcherResult> Match((int start, int length, string text) input)
        {
            var cp = char.ConvertToUtf32(input.text, 0);
            return names.GetValueOrNone(input.text)
                .Else(lookup.GetValueOrNone(cp))
                .Map(radical => new RadicalSearcherResult(input.start, input.length, input.text, radical));
        }

        /*private class LexemeBuilder
        {
            private string input;

            private int start = 0;

            private StringBuilder sb;

            public LexemeBuilder(string input, StringBuilder sb = null)
            {
                this.input = input;
                sb = sb ?? new StringBuilder();
                sb.Clear();
                this.sb = sb;
            }

            public bool Next()
            {
                if (Index >= input.Length)
                    return false;

                Index++;
                if (char.IsHighSurrogate(input, Index))
                    Index++;
                return true;
            }

            public int CodePoint { get; private set; }

            public int Index { get; private set; } = -1;

            public void Feed(int codePoint)
            {
                sb.AppendCodePoint(codePoint);
            }

            public bool Empty => sb.Length == 0;

            public (int start, int length, string text) FlushRaw()
            {

            }
        }*/

        private IEnumerable<(int start, int length, string text)> Split(string text)
        {
            int currentState = 0;
            var sb = new StringBuilder();
            text += " "; // sentinel value
            foreach (var i in text.AsCodePointIndices())
            {
                var cp = char.ConvertToUtf32(text, i);
                var blockName = UnicodeInfo.GetBlockName(cp);
                if (cp == '|' || cp == '｜' || blockName == "CJK Unified Ideographs")
                {
                    { if (Flush(i, out var token)) yield return token; }
                    sb.AppendCodePoint(char.ConvertToUtf32(text, i));
                    { if (Flush(i, out var token)) yield return token; }
                }
                else if (char.IsWhiteSpace(text, i) || char.IsPunctuation(text, i))
                {
                    { if (Flush(i + 1, out var token)) yield return token; }
                }
                else
                {
                    sb.AppendCodePoint(char.ConvertToUtf32(text, i));
                }
            }

            bool Flush(int i, out (int start, int length, string text) v)
            {
                v = default;
                if (sb.Length == 0)
                {
                    currentState = i;
                    return false;
                }

                v = (currentState, sb.Length, sb.ToString());
                currentState = i+1;
                sb.Clear();
                return true;
            }
        }

        public RadicalSearcher(IEnumerable<CodePoint> radicals, IEnumerable<KanjiAliveJapaneseRadicalInformation.Entry> radicalInfoEntries, IReadOnlyDictionary<int, int> remapper)
        {
            radicals = radicals.Materialize();

            this.lookup = radicals.ToDictionary(r => r.Utf32, r => r);
            var correlated = radicals
                .Join(
                    radicalInfoEntries.Where(e => e.StrokeCount.HasValue),
                    radical => remapper.GetValueOrNone(radical.Utf32).ValueOr(0),
                    radicalInfo => char.ConvertToUtf32(radicalInfo.Literal, 0),
                    (radical, radicalInfo) => (
                        names: radicalInfo.Meanings.Concat(radicalInfo.JapaneseReadings)
                            .Concat(radicalInfo.RomajiReadings),
                        radical: radical));
            var kvps = correlated
                .SelectMany(p => p.names.Select(n => new KeyValuePair<string, CodePoint>(n, p.radical)));
            this.names = kvps
                .ToDictionary((k, ov, nv) => ov);
        }
    }
}
