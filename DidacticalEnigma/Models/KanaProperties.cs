using System.Collections.Generic;

namespace DidacticalEnigma.Models
{
    public static class KanaProperties
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

        public static int? OppositeSizedVersionOf(int codePoint)
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

        public static int? LargeKanaOf(int codePoint)
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

        public static int? SmallKanaOf(int codePoint)
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
    }
}
