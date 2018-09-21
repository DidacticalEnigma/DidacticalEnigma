using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DidacticalEnigma.Core.Utils;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class KanaProperties : ISimilarKana
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

        public KanaProperties(string katakanaPath, string hiraganaPath, string hiraganaKatakanaPath, string complexPath, Encoding encoding)
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
}
