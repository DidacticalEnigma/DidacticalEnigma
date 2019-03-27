using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Unicode;
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
    }

    public interface IRadicalSearcher
    {
        IReadOnlyList<RadicalSearcherResult> Search(string text);
    }

    public class RadicalSearcher : IRadicalSearcher
    {
        private Dictionary<int, CodePoint> lookup;

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
            return lookup.GetValueOrNone(cp).Map(radical => new RadicalSearcherResult(input.start, input.length, input.text, radical));
        }

        private IEnumerable<(int start, int length, string text)> Split(string text)
        {
            var sb = new StringBuilder();
            foreach (var i in text.AsCodePointIndices())
            {
                if (char.IsWhiteSpace(text, i))
                {
                    if (Flush(i, out var token)) yield return token;
                }
                else
                {
                    sb.AppendCodePoint(char.ConvertToUtf32(text, i));
                    if (Flush(i, out var token)) yield return token;
                }
            }

            bool Flush(int i, out (int start, int length, string text) v)
            {
                v = default;
                if (sb.Length == 0)
                    return false;
                v = (i, sb.Length, sb.ToString());
                sb.Clear();
                return true;
            }
        }

        public RadicalSearcher(IEnumerable<CodePoint> radicals)
        {
            this.lookup = radicals.ToDictionary(r => r.Utf32, r => r);
        }
    }
}
