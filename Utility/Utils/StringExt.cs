using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility.Utils
{
    public static class StringExt
    {
        public static IEnumerable<string> AllRotationsOf(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            if (s.Length == 0)
                yield break;

            var queue = new Queue<int>(s.AsCodePoints());
            string result;
            do
            {
                queue.Enqueue(queue.Dequeue());
                result = FromCodePoints(queue);
                yield return result;
            } while (result != s);
        }

        public static IEnumerable<int> AsCodePoints(this string s)
        {
            for (int i = 0; i < s.Length; ++i)
            {
                yield return Char.ConvertToUtf32(s, i);
                if (Char.IsHighSurrogate(s, i))
                    i++;
            }
        }

        public static IEnumerable<int> AsCodePointIndices(this string s)
        {
            for (int i = 0; i < s.Length; ++i)
            {
                yield return i;
                if (Char.IsHighSurrogate(s, i))
                    i++;
            }
        }

        public static StringBuilder AppendCodePoint(this StringBuilder stringBuilder, int codePoint)
        {
            int utf32 = codePoint;
            if ((utf32 < 0 || utf32 > 0x10ffff) || (utf32 >= 0x00d800 && utf32 <= 0x00dfff))
            {
                throw new ArgumentException(nameof(codePoint));
            }

            if (utf32 < 0x10000)
            {
                stringBuilder.Append((char)codePoint);
            }
            else
            {
                utf32 -= 0x10000;
                stringBuilder.Append((char)((utf32 / 0x400) + '\ud800'));
                stringBuilder.Append((char)((utf32 % 0x400) + '\udc00'));
            }

            return stringBuilder;
        }

        public static string SubstringFromTo(this string s, int start, int end)
        {
            return s.Substring(start, end - start);
        }

        public static IEnumerable<(string text, bool highlight)> HighlightWords(string input, string word)
        {
            return input.Split(new[] { word }, StringSplitOptions.None).Select(part => (part, false)).Intersperse((word, true));
        }

        public static string FromCodePoints(IEnumerable<int> codePoints)
        {
            var sb = new StringBuilder();
            foreach(var codePoint in codePoints)
            {
                sb.AppendCodePoint(codePoint);
            }
            return sb.ToString();
        }

        public static bool TryGetPrefix(this string input, string prefix, out string tail)
        {
            return TryGetPrefix(input, prefix, StringComparison.Ordinal, out tail);
        }

        public static bool TryGetPrefix(this string input, string prefix, StringComparison comparison, out string tail)
        {
            if (input.StartsWith(prefix, comparison))
            {
                tail = input.Substring(prefix.Length);
                return true;
            }

            tail = "";
            return false;
        }

        public static IEnumerable<string> SplitWithQuotes(string input, char delimiter, char quote)
        {
            var sb = new StringBuilder();
            bool isQuoted = false;
            foreach (var c in input)
            {
                if (c == delimiter && !isQuoted)
                {
                    yield return sb.ToString();
                    sb.Clear();
                }
                else if (c == quote)
                {
                    isQuoted = !isQuoted;
                }
                else
                {
                    sb.Append(c);
                }
            }

            yield return sb.ToString();
        }
    }
}
