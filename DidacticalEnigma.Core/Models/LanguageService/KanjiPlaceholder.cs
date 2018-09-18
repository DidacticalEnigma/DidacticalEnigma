using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DidacticalEnigma.Core.Utils;
using JDict;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    class KanjiPlaceholder
    {
        public IEnumerable<Radical> Radicals { get; }

        public CodePoint MappedCodePoint { get; }
    }

    static class KanjiPlaceholderExtensions
    {
        public static Func<string, bool> CreateMatcher(
            this ILanguageService lang,
            IReadOnlyDictionary<CodePoint, KanjiPlaceholder> haystack,
            string template)
        {
            var privateUseAreaMatch = new Regex(@"\p{Co}");
            var regex = new Regex("^" + privateUseAreaMatch.Replace(Regex.Escape(template).Replace(@"/\\", "."), ".") + "$");
            return word => regex.IsMatch(word) && KanjiPlaceholdersMatch(template, word);

            bool KanjiPlaceholdersMatch(string t, string c)
            {
                t = t.Replace(@"/\\", ".");
                foreach (var (templateChar, concreteChar) in t.AsCodePoints().Zip(c.AsCodePoints(),
                    (l, r) => (CodePoint.FromInt(l), CodePoint.FromInt(r))))
                {
                    if (!haystack.TryGetValue(templateChar, out var placeholder))
                        continue;

                    var concreteRadicals = new HashSet<CodePoint>(concreteChar is Kanji k
                        ? lang.LookupRadicals(k)
                        : Enumerable.Empty<CodePoint>());
                    if (placeholder.Radicals.All(templateRadical =>
                        concreteRadicals.Contains(templateRadical.CodePoint)))
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
