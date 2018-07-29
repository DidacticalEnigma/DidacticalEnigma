using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidacticalEnigma.Models
{
    class SimilarKana
    {
        private Dictionary<CodePoint, List<CodePoint>> similarityGroups;

        private static IReadOnlyDictionary<Type, int> position = new Dictionary<Type, int>
        {
            { typeof(Hiragana), 1 },
            { typeof(Katakana), 2 },
            { typeof(Kanji), 4 },
            { typeof(CodePoint), 8 },
        };

        public IEnumerable<CodePoint> FindSimilar(CodePoint point)
        {
            similarityGroups.TryGetValue(point, out var listOfSimilar);
            var similar = listOfSimilar ?? Enumerable.Empty<CodePoint>();
            return similar
                .Except(Enumerable.Repeat(point, 1))
                .OrderBy(other =>
                {
                    return Math.Abs(position[point.GetType()] - position[other.GetType()]);
                });
        }

        private SimilarKana(IEnumerable<IEnumerable<CodePoint>> input)
        {
            var similaritySets = new ConcurrentDictionary<CodePoint, UnionFindNode>();
            foreach (var group in input)
            {
                UnionFindNode first = null;
                foreach (var cp in group)
                {
                    var set = similaritySets.GetOrAdd(cp, x => new UnionFindNode());
                    if(first != null)
                    {
                        set.Union(first);
                    }
                    first = set;
                }
            }
            var uniqueLists = new Dictionary<UnionFindNode, List<CodePoint>>();
            foreach(var kvp in similaritySets)
            {
                uniqueLists[kvp.Value.Find()] = new List<CodePoint>();
            }
            similarityGroups = new Dictionary<CodePoint, List<CodePoint>>();
            foreach(var kvp in similaritySets)
            {
                var list = uniqueLists[similaritySets[kvp.Key].Find()];
                list.Add(kvp.Key);
                similarityGroups[kvp.Key] = list;
            }
        }

        public static SimilarKana FromFile(string path)
        {
            return new SimilarKana(
                File.ReadLines(path, Encoding.UTF8)
                    .Select(line => line.AsCodePoints().Select(cp => CodePoint.FromInt(cp))));
            
        }
    }
}
