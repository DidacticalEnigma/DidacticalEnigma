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

        private readonly IReadOnlyDictionary<int, int> remapper;

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
            var prev = -1;
            foreach (var i in text.AsCodePointIndices())
            {
                var previousCp = prev == -1 ? 0 : char.ConvertToUtf32(text, prev);
                var cp = char.ConvertToUtf32(text, i);

                if (prev != -1)
                {
                    sb.AppendCodePoint(previousCp);
                }
                if (prev != -1 && IsBoundary(text, prev, i, previousCp, cp))
                {
                    (int start, int length, string text) token = (currentState, sb.Length, sb.ToString());
                    currentState += sb.Length;
                    sb.Clear();
                    if(ShouldEmit(token.text))
                        yield return token;
                }

                prev = i;
            }

            bool IsBoundary(string t, int previousIndex, int currentIndex, int previousCodePoint, int currentCodePoint)
            {
                if (previousCodePoint == '|' || previousCodePoint == '｜' || UnicodeInfo.GetBlockName(previousCodePoint) == "CJK Unified Ideographs")
                    return true;

                if (currentCodePoint == '|' || currentCodePoint == '｜' || UnicodeInfo.GetBlockName(currentCodePoint) == "CJK Unified Ideographs")
                    return true;

                if ((char.IsPunctuation(t, previousIndex) || char.IsWhiteSpace(t, previousIndex)) &&
                    !(char.IsPunctuation(t, currentIndex) || char.IsWhiteSpace(t, currentIndex)))
                {
                    return true;
                }

                if (!(char.IsPunctuation(t, previousIndex) || char.IsWhiteSpace(t, previousIndex)) &&
                    (char.IsPunctuation(t, currentIndex) || char.IsWhiteSpace(t, currentIndex)))
                {
                    return true;
                }

                return false;
            }

            bool ShouldEmit(string t)
            {
                if (t.AsCodePointIndices().Any(i => char.IsPunctuation(t, i) || char.IsWhiteSpace(t, i)))
                    return false;
                return true;
            }
        }

        public RadicalSearcher(IEnumerable<CodePoint> radicals, IEnumerable<KanjiAliveJapaneseRadicalInformation.Entry> radicalInfoEntries, IReadOnlyDictionary<int, int> remapper)
        {
            radicals = radicals.Materialize();
            this.remapper = remapper;
            this.lookup = radicals.ToDictionary(r => r.Utf32, r => r);
            var correlated = radicals
                .Join(
                    radicalInfoEntries.Where(e => e.StrokeCount.HasValue),
                    radical => remapper.GetValueOrNone(radical.Utf32).ValueOr(0),
                    radicalInfo => char.ConvertToUtf32(radicalInfo.Literal, 0),
                    (radical, radicalInfo) => (
                        names: radicalInfo.Meanings.Concat(radicalInfo.JapaneseReadings)
                            .Concat(radicalInfo.RomajiReadings).Concat(new []{radicalInfo.Literal}),
                        radical: radical));
            var kvps = correlated
                .SelectMany(p => p.names.Select(n => new KeyValuePair<string, CodePoint>(n, p.radical)));
            this.names = kvps
                .ToDictionary((k, ov, nv) => ov);
        }
    }
}
