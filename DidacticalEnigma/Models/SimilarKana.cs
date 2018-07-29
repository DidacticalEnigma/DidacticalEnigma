using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidacticalEnigma.Models
{
    class SimilarKana
    {
        private Dictionary<CodePoint, IEnumerable<CodePoint>> similarityGroups;

        private static IReadOnlyDictionary<Type, int> position = new Dictionary<Type, int>
        {
            { typeof(Hiragana), 1 },
            { typeof(Katakana), 2 },
            { typeof(Kanji), 4 },
            { typeof(CodePoint), 8 },
        };

        IEnumerable<CodePoint> FindSimilar(CodePoint point)
        {
            similarityGroups.TryGetValue(point, out var similar);
            similar = similar ?? Enumerable.Empty<CodePoint>();
            return similar
                .Except(Enumerable.Repeat(point, 1))
                .OrderBy(other =>
                {
                    return Math.Abs(position[point.GetType()] - position[other.GetType()]);
                });
        }
    }
}
