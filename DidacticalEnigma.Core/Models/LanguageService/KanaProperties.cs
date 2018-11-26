using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Optional;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IKanaProperties
    {
        int? OppositeSizedVersionOf(int codePoint);
        int? LargeKanaOf(int codePoint);
        int? SmallKanaOf(int codePoint);
        string ToHiragana(string input);
        string LookupRomaji(string s);
    }

    public class KanaProperties2 : IKanaProperties, IRelated
    {
        public string LookupRomaji(string s)
        {
            kanaToRomaji.TryGetValue(s, out var value);
            return value;
        }

        public int? OppositeSizedVersionOf(int codePoint)
        {
            if (smallLargeMap.TryGetKey(codePoint, out var outKana))
            {
                return outKana;
            }
            else if (smallLargeMap.TryGetValue(codePoint, out outKana))
            {
                return outKana;
            }
            else
            {
                return null;
            }
        }

        public int? LargeKanaOf(int codePoint)
        {
            if (smallLargeMap.TryGetValue(codePoint, out var outKana))
            {
                return outKana;
            }
            else
            {
                return null;
            }
        }

        public int? SmallKanaOf(int codePoint)
        {
            if (smallLargeMap.TryGetKey(codePoint, out var outKana))
            {
                return outKana;
            }
            else
            {
                return null;
            }
        }

        public string ToHiragana(string input)
        {
            return StringExt.FromCodePoints(input.AsCodePoints().Select(c =>
            {
                if (hiraganaKatakanaMap.TryGetKey(c, out var hiraganaCodePoint))
                {
                    return hiraganaCodePoint;
                }
                else
                {
                    // maybe it's a small version?
                    var large = OppositeSizedVersionOf(c);
                    if (large.HasValue && hiraganaKatakanaMap.TryGetKey(large.Value, out hiraganaCodePoint))
                    {
                        return OppositeSizedVersionOf(large.Value).Value;
                    }
                    else
                    {
                        return c;
                    }
                }
            }));
        }

        private readonly Dictionary<string, string> kanaToRomaji;
        private readonly DualDictionary<int, int> regularHandakutenMap;
        private readonly DualDictionary<int, int> regularDakutenMap;
        private readonly DualDictionary<int, int> smallLargeMap;
        private readonly DualDictionary<int, int> hiraganaKatakanaMap;

        public KanaProperties2(string kanaPropertiesPath, Encoding encoding)
        {
            var kanaToRomaji = new Dictionary<string, string>();
            var regularHandakutenMap = new Dictionary<int, int>();
            var regularDakutenMap = new Dictionary<int, int>();
            var smallLargeMap = new Dictionary<int, int>();
            var katakanaHiraganaMap = new Dictionary<int, int>();
            var hiraganaKatakanaMap = new Dictionary<int, int>();
            foreach (var line in File.ReadLines(kanaPropertiesPath, encoding))
            {
                var components = line.Split(' ');
                var hiragana = components[0];
                var katakana = components[1];
                var romaji = components[2];
                var kind = components[3];
                switch (kind)
                {
                    case "regular":
                    case "archaic":
                        AddHiraganaKatakanaMapping(katakanaHiraganaMap, hiraganaKatakanaMap, katakana, hiragana);
                        AddToRomajiDic(kanaToRomaji, hiragana, katakana, romaji);
                        break;
                    case "combo":
                        AddToRomajiDic(kanaToRomaji, hiragana, katakana, romaji);
                        break;
                    case "small":
                        smallLargeMap.Add(AssumeSingleCodepoint(hiragana), AssumeSingleCodepoint(components[4]));
                        smallLargeMap.Add(AssumeSingleCodepoint(katakana), hiraganaKatakanaMap[AssumeSingleCodepoint(components[4])]);
                        break;
                    case "dakuten":
                        regularDakutenMap.Add(AssumeSingleCodepoint(components[4]), AssumeSingleCodepoint(hiragana));
                        regularDakutenMap.Add(hiraganaKatakanaMap[AssumeSingleCodepoint(components[4])], AssumeSingleCodepoint(katakana));
                        AddHiraganaKatakanaMapping(katakanaHiraganaMap, hiraganaKatakanaMap, katakana, hiragana);
                        AddToRomajiDic(kanaToRomaji, hiragana, katakana, romaji);
                        break;
                    case "handakuten":
                        regularHandakutenMap.Add(AssumeSingleCodepoint(components[4]), AssumeSingleCodepoint(hiragana));
                        regularHandakutenMap.Add(hiraganaKatakanaMap[AssumeSingleCodepoint(components[4])], AssumeSingleCodepoint(katakana));
                        AddHiraganaKatakanaMapping(katakanaHiraganaMap, hiraganaKatakanaMap, katakana, hiragana);
                        AddToRomajiDic(kanaToRomaji, hiragana, katakana, romaji);
                        break;
                    default:
                        throw new InvalidDataException();
                }
            }

            this.kanaToRomaji = kanaToRomaji;
            this.regularHandakutenMap = new DualDictionary<int, int>(regularHandakutenMap);
            this.regularDakutenMap = new DualDictionary<int, int>(regularDakutenMap);
            this.smallLargeMap = new DualDictionary<int, int>(smallLargeMap);
            this.hiraganaKatakanaMap = new DualDictionary<int, int>(hiraganaKatakanaMap);

            void AddToRomajiDic(Dictionary<string, string> map, string hiragana, string katakana, string romaji)
            {
                map.Add(hiragana, romaji);
                map.Add(katakana, romaji);
            }

            void AddHiraganaKatakanaMapping(Dictionary<int, int> dictionary, Dictionary<int, int> ints, string katakana, string hiragana)
            {
                dictionary.Add(AssumeSingleCodepoint(katakana), AssumeSingleCodepoint(hiragana));
                ints.Add(AssumeSingleCodepoint(hiragana), AssumeSingleCodepoint(katakana));
            }
        }

        int AssumeSingleCodepoint(string s)
        {
            if (s.Length <= 0 || s.Length > 2)
                throw new ArgumentException();

            if (s.Length == 2 && !char.IsHighSurrogate(s[0]))
                throw new ArgumentException();

            return char.ConvertToUtf32(s, 0);
        }

        public IEnumerable<IGrouping<string, CodePoint>> FindRelated(CodePoint codePoint)
        {
            var result = new List<IGrouping<string, CodePoint>>();
            if (hiraganaKatakanaMap.TryGetValue(codePoint.Utf32, out var katakana))
            {
                result.Add(new CategoryGrouping<CodePoint>("Katakana", new[] { CodePoint.FromInt(katakana), }));
            }
            if (hiraganaKatakanaMap.TryGetKey(codePoint.Utf32, out var hiragana))
            {
                result.Add(new CategoryGrouping<CodePoint>("Hiragana", new[] { CodePoint.FromInt(hiragana), }));
            }
            if (smallLargeMap.TryGetValue(codePoint.Utf32, out var large))
            {
                result.Add(new CategoryGrouping<CodePoint>("Large", new []{CodePoint.FromInt(large), }));
            }
            if (smallLargeMap.TryGetKey(codePoint.Utf32, out var small))
            {
                result.Add(new CategoryGrouping<CodePoint>("Small", new[] { CodePoint.FromInt(small), }));
            }
            if (regularDakutenMap.TryGetValue(codePoint.Utf32, out var dakuten) ||
                (regularHandakutenMap.TryGetKey(codePoint.Utf32, out var r1) &&
                 regularDakutenMap.TryGetValue(r1, out dakuten)))
            {
                result.Add(new CategoryGrouping<CodePoint>("Dakuten", new []{ CodePoint.FromInt(dakuten), }));
            }
            if (regularHandakutenMap.TryGetValue(codePoint.Utf32, out var handakuten) ||
                (regularDakutenMap.TryGetKey(codePoint.Utf32, out var r2) && 
                 regularHandakutenMap.TryGetValue(r2, out handakuten)))
            {
                result.Add(new CategoryGrouping<CodePoint>("Handakuten", new[] { CodePoint.FromInt(handakuten), }));
            }
            if (regularDakutenMap.TryGetKey(codePoint.Utf32, out var regular) ||
                regularHandakutenMap.TryGetKey(codePoint.Utf32, out regular))
            {
                result.Add(new CategoryGrouping<CodePoint>("Regular", new[] { CodePoint.FromInt(regular), }));
            }

            return result;
        }
    }
}
