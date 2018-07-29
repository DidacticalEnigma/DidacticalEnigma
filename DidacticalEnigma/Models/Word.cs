using JDict;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Unicode;

namespace DidacticalEnigma.Models
{
    public class WordVM
    {
        public enum WordKind
        {
            Unknown,
            Particle,
            Noun,
            Verb,
        }

        public string DictionaryStringBlurb { get; }

        public EDictEntry DictionaryEntry { get; }

        public string StringForm { get; }

        public WordKind Kind { get; }

        public WordVM(string s, EDictEntry entry)
        {
            StringForm = s;
            DictionaryEntry = entry;
            DictionaryStringBlurb = entry?.ToString();
        }
    }

    public class CodePointVM
    {
        public CodePoint CodePoint { get; }

        public string FullName => CodePoint.ToLongString();

        public string StringForm => CodePoint.ToString();

        public WordVM Word { get; }

        public IEnumerable<CodePoint> Similar { get; }

        public bool HasSimilar => Similar.Any();

        public CodePointVM(CodePoint cp, WordVM word, IEnumerable<CodePoint> similar)
        {
            CodePoint = cp;
            Word = word;
            Similar = similar;
        }
    }

    // To check: do I need normalization forms?
    public class CodePoint : IEquatable<CodePoint>
    {
        protected readonly int codePoint;

        public string Name => UnicodeInfo.GetCharInfo(codePoint).Name;

        public string ToLongString()
        {
            return $"{char.ConvertFromUtf32(codePoint)}: {Name}";
        }

        public override string ToString()
        {
            return char.ConvertFromUtf32(codePoint);
        }

        internal CodePoint(int s)
        {
            codePoint = s;
        }

        public static CodePoint FromInt(int codePoint)
        {
            var info = UnicodeInfo.GetCharInfo(codePoint);
            switch(info.Block)
            {
                case "Hiragana":
                    return new Hiragana(codePoint);
                case "Katakana":
                    return new Katakana(codePoint);
                case "Kanji":
                    return new Kanji(codePoint);
                default:
                    return new CodePoint(codePoint);
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CodePoint);
        }

        public bool Equals(CodePoint other)
        {
            return other != null &&
                   codePoint == other.codePoint;
        }

        public override int GetHashCode()
        {
            return -1644926438 + codePoint.GetHashCode();
        }

        public static bool operator ==(CodePoint point1, CodePoint point2)
        {
            return EqualityComparer<CodePoint>.Default.Equals(point1, point2);
        }

        public static bool operator !=(CodePoint point1, CodePoint point2)
        {
            return !(point1 == point2);
        }
    }
    
    public class Kanji : CodePoint
    {
        internal Kanji(int s) :
            base(s)
        {
            
        }
    }

    public abstract class Kana : CodePoint
    {
        internal Kana(int s) :
            base(s)
        {

        }

        public bool HasOppositeSizedVersion => KanaProperties.OppositeSizedVersionOf(codePoint).HasValue;

        public int OppositeSizedVersion => KanaProperties.OppositeSizedVersionOf(codePoint).Value;
    }

    public class Hiragana : Kana
    {
        internal Hiragana(int s) :
            base(s)
        {

        }
    }

    public class Katakana : Kana
    {
        internal Katakana(int s) :
            base(s)
        {

        }
    }
}
