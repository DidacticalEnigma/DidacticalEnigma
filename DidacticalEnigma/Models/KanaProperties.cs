using DidacticalEnigma.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DidacticalEnigma.Models
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
        });

        public Dictionary<string, string> romajiMapping = new Dictionary<string, string>();

        public Dictionary<string, List<string>> mapping = new Dictionary<string, List<string>>();

        public KanaProperties(string katakanaPath, string hiraganaPath, string complexPath, Encoding encoding)
        {
            ReadKanaFile(katakanaPath);
            ReadKanaFile(hiraganaPath);

            foreach (var lineColumn in File.ReadLines(complexPath, encoding))
            {
                var components = lineColumn.Split(' ');
                if (components.Length > 2)
                    romajiMapping.Add(components[1], components[2]);
                var list = mapping.GetOrAdd(components[0], () => new List<string>());
                list.Add(components[1]);
            }

            void ReadKanaFile(string kanaPath)
            {
                foreach (var lineColumn in File.ReadLines(kanaPath, encoding))
                {
                    var components = lineColumn.Split(' ');
                    if (components.Length > 1)
                        romajiMapping.Add(components[0], components[1]);
                }
            }
        }

        public int? OppositeSizedVersionOf(int codePoint)
        {
            if (smallLargeVersions.TryGetKey(codePoint, out var outKana))
            {
                return outKana;
            }
            else if (smallLargeVersions.TryGetValue(codePoint, out outKana))
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
            if (smallLargeVersions.TryGetKey(codePoint, out var outKana))
            {
                return outKana;
            }
            else
            {
                return null;
            }
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
