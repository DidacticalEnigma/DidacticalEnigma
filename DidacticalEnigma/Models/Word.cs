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

        public string StringForm => CodePoint.ToString();

        public WordVM Word { get; }

        public IEnumerable<CodePoint> Similar { get; }

        public CodePointVM(CodePoint cp, WordVM word, IEnumerable<CodePoint> similar)
        {
            CodePoint = cp;
            Word = word;
            Similar = similar.ToList();
        }
    }

    // To check: do I need normalization forms?
    public class CodePoint
    {
        private int codePoint;

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
    }
    
    public class Kanji : CodePoint
    {
        internal Kanji(int s) :
            base(s)
        {
            
        }
    }

    public class Hiragana : CodePoint
    {
        internal Hiragana(int s) :
            base(s)
        {

        }
    }

    public class Katakana : CodePoint
    {
        internal Katakana(int s) :
            base(s)
        {

        }
    }
}
