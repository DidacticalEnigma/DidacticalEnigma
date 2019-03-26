using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JDict;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class PartialWordLookup
    {
        private string allWords;

        private static readonly char start = '\x0002';

        private static readonly char[] startArr = { start };

        private static readonly char end = '\x0001';

        private static readonly char[] endArr = { end };

        public PartialWordLookup(JMDict jmDict)
        {
            allWords = string.Join("", jmDict.AllEntries()
                .SelectMany(entry => entry.Kanji.Concat(entry.Readings))
                .Distinct()
                .Select(word => start + word + end));
        }

        public IEnumerable<string> LookupWords(string input)
        {
            var regex = new Regex(
                Regex.Escape(start + input + end)
                    .Replace(@"\?", ".")
                    .Replace(@"？", ".")
                    .Replace(@"\*", ".*?")
                    .Replace(@"＊", ".*?"));
            return regex.Matches(allWords)
                .Cast<Match>()
                .Select(m => m.Value.TrimStart(startArr).TrimEnd(endArr))
                .ToList();
        }
    }
}
