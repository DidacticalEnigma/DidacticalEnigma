using System;
using System.Collections.Generic;
using System.Unicode;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    // To check: do I need normalization forms?
    public class CodePoint : IEquatable<CodePoint>
    {
        public int Utf32 { get; }

        public string Name => UnicodeInfo.GetCharInfo(Utf32).Name;

        public string ToLongString()
        {
            return $"{char.ConvertFromUtf32(Utf32)} ({Utf32}): {Name}";
        }

        public virtual string ToDescriptionString()
        {
            return ToLongString();
        }

        public string StringRepresentation => char.ConvertFromUtf32(Utf32);

        public override string ToString()
        {
            return StringRepresentation;
        }

        internal CodePoint(int s)
        {
            Utf32 = s;
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
                case "CJK Unified Ideographs":
                    return new Kanji(codePoint);
                default:
                    return new CodePoint(codePoint);
            }
        }

        public static CodePoint FromString(string codePoint, int position = 0)
        {
            return FromInt(char.ConvertToUtf32(codePoint, position));
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CodePoint);
        }

        public bool Equals(CodePoint other)
        {
            return other != null &&
                   Utf32 == other.Utf32;
        }

        public override int GetHashCode()
        {
            return -1644926438 + Utf32.GetHashCode();
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
}
