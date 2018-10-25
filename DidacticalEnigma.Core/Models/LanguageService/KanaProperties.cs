using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DidacticalEnigma.Core.Utils;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IKanaProperties : ISimilarKana
    {
        int? OppositeSizedVersionOf(int codePoint);
        int? LargeKanaOf(int codePoint);
        int? SmallKanaOf(int codePoint);
        string ToHiragana(string input);
        string LookupRomaji(string s);
    }

    public class KanaProperties : IKanaProperties
    {
        private static readonly DualDictionary<int, int> smallLargeVersions = new DualDictionary<int, int>(new Dictionary<int, int>
        {
            // hiragana
            { 'ぁ', 'あ' },
            { 'ぃ' ,'い' },
            { 'ぅ' ,'う' },
            { 'ぇ' ,'え' },
            { 'ぉ' ,'お' },
            { 'っ' ,'つ' },
            { 'ゃ' ,'や' },
            { 'ゅ' ,'ゆ' },
            { 'ょ' ,'よ' },
            { 'ゎ' ,'わ' },
            { 'ゕ' ,'か' },
            // katakana
            { 'ァ' ,'ア' },
            { 'ィ' ,'イ' },
            { 'ゥ' ,'ウ' },
            { 'ェ' ,'エ' },
            { 'ォ' ,'オ' },
            { 'ッ' ,'ツ' },
            { 'ュ' ,'ユ' },
            { 'ョ' ,'ヨ' },
            { 'ヵ' ,'カ' },
            { 'ャ' ,'ヤ' },
            { 'ヮ', 'ワ' }
        });

        private Dictionary<string, string> romajiMapping = new Dictionary<string, string>();

        private Dictionary<string, List<string>> mapping = new Dictionary<string, List<string>>();

        private DualDictionary<int, int> hiraganaKatakana;

        public KanaProperties(string hiraganaPath, string katakanaPath, string hiraganaKatakanaPath, string complexPath, Encoding encoding)
        {
            ReadKanaFile(katakanaPath);
            ReadKanaFile(hiraganaPath);

            foreach(var lineColumn in File.ReadLines(complexPath, encoding))
            {
                var components = lineColumn.Split(' ');
                if(components.Length > 2)
                    romajiMapping.Add(components[1], components[2]);
                var list = mapping.GetOrAdd(components[0], () => new List<string>());
                list.Add(components[1]);
            }

            var kana = new Dictionary<int, int>();
            foreach(var lineColumn in File.ReadLines(hiraganaKatakanaPath, encoding))
            {
                var components = lineColumn.Split(' ');
                if(components.Length > 1)
                    kana.Add(components[0].AsCodePoints().Single(), components[1].AsCodePoints().Single());
            }
            hiraganaKatakana = new DualDictionary<int, int>(kana);

            void ReadKanaFile(string kanaPath)
            {
                foreach(var lineColumn in File.ReadLines(kanaPath, encoding))
                {
                    var components = lineColumn.Split(' ');
                    if(components.Length > 1)
                        romajiMapping.Add(components[0], components[1]);
                }
            }
        }

        public int? OppositeSizedVersionOf(int codePoint)
        {
            if(smallLargeVersions.TryGetKey(codePoint, out var outKana))
            {
                return outKana;
            }
            else if(smallLargeVersions.TryGetValue(codePoint, out outKana))
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
            if(smallLargeVersions.TryGetValue(codePoint, out var outKana))
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
            if(smallLargeVersions.TryGetKey(codePoint, out var outKana))
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
                if(hiraganaKatakana.TryGetKey(c, out var hiraganaCodePoint))
                {
                    return hiraganaCodePoint;
                }
                else
                {
                    // maybe it's a small version?
                    var large = OppositeSizedVersionOf(c);
                    if(large.HasValue && hiraganaKatakana.TryGetKey(large.Value, out hiraganaCodePoint))
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

        public string LookupRomaji(string s)
        {
            if(s.Length == 1)
            {
                var largeOpt = LargeKanaOf(char.ConvertToUtf32(s, 0));
                s = largeOpt != null ? char.ConvertFromUtf32(largeOpt.Value) : s;
            }
            romajiMapping.TryGetValue(s, out var value);
            return value;
        }

        public IEnumerable<CodePoint> FindSimilar(CodePoint point)
        {
            var oppositeSizedCp = OppositeSizedVersionOf(point.Utf32);
            var oppositeSized = (oppositeSizedCp != null
                ? Enumerable.Repeat(char.ConvertFromUtf32(oppositeSizedCp.Value), 1)
                : Enumerable.Empty<string>())
                .Select(s => CodePoint.FromString(s));

            mapping.TryGetValue(point.ToString(), out var restStr);
            restStr = restStr ?? new List<string>();

            // TOFIX: support combo kana
            var rest = restStr
                .Where(s => s.Length == 1)
                .Select(s => CodePoint.FromString(s));

            return oppositeSized.Concat(rest);
        }
    }

    public class KanaProperties2 : IKanaProperties
    {
        public IEnumerable<CodePoint> FindSimilar(CodePoint point)
        {
            return Enumerable.Empty<CodePoint>();
        }

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
                        katakanaHiraganaMap.Add(AssumeSingleCodepoint(katakana), AssumeSingleCodepoint(hiragana));
                        hiraganaKatakanaMap.Add(AssumeSingleCodepoint(hiragana), AssumeSingleCodepoint(katakana));
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
                        AddToRomajiDic(kanaToRomaji, hiragana, katakana, romaji);
                        break;
                    case "handakuten":
                        regularHandakutenMap.Add(AssumeSingleCodepoint(components[4]), AssumeSingleCodepoint(hiragana));
                        regularHandakutenMap.Add(hiraganaKatakanaMap[AssumeSingleCodepoint(components[4])], AssumeSingleCodepoint(katakana));
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
        }

        int AssumeSingleCodepoint(string s)
        {
            if (s.Length <= 0 || s.Length > 2)
                throw new ArgumentException();

            if(s.Length == 2 && !char.IsHighSurrogate(s[0]))
                throw new ArgumentException();

            return char.ConvertToUtf32(s, 0);
        }
    }
}
