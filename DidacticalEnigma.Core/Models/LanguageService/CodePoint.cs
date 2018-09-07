using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Unicode;

namespace DidacticalEnigma.Models
{

    // To check: do I need normalization forms?
    public class CodePoint : IEquatable<CodePoint>
    {
        protected readonly int codePoint;

        public int Utf32 => codePoint;

        public string Name => UnicodeInfo.GetCharInfo(codePoint).Name;

        public string ToLongString()
        {
            return $"{char.ConvertFromUtf32(codePoint)} ({codePoint}): {Name}";
        }

        public virtual string ToDescriptionString()
        {
            return ToLongString();
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
}
