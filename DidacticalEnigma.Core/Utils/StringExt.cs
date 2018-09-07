using System;
using System.Collections.Generic;
using System.Linq;

namespace DidacticalEnigma.Core.Utils
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
    }
}
