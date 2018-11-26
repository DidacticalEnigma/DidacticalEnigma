using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility.Utils
{
    public static class StringExt
    {
        public static IEnumerable<int> AsCodePoints(this string s)
        {
            for (int i = 0; i < s.Length; ++i)
            {
                yield return Char.ConvertToUtf32(s, i);
                if (Char.IsHighSurrogate(s, i))
                    i++;
            }
        }

        public static string SubstringFromTo(this string s, int start, int end)
        {
            return s.Substring(start, end - start);
        }

        public static IEnumerable<(string text, bool highlight)> HighlightWords(string input, string word)
        {
            return input.Split(new string[] { word }, StringSplitOptions.None).Select(part => (part, false)).Intersperse((word, true));
        }

        public static string FromCodePoints(IEnumerable<int> codePoints)
        {
            var list = new List<char>();
            foreach(var codePoint in codePoints)
            {
                int utf32 = codePoint;
                if((utf32 < 0 || utf32 > 0x10ffff) || (utf32 >= 0x00d800 && utf32 <= 0x00dfff))
                {
                    throw new ArgumentException(nameof(codePoints));
                }

                if(utf32 < 0x10000)
                {
                    list.Add((char)codePoint);
                }
                else
                {
                    utf32 -= 0x10000;
                    list.Add((char)((utf32 / 0x400) + '\ud800'));
                    list.Add((char)((utf32 % 0x400) + '\udc00'));
                }
            }
            return new string(list.ToArray());
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
