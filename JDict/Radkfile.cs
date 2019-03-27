using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JDict
{
    public struct Radical
    {
        public int CodePoint { get; }

        public int StrokeCount { get; }

        public Radical(int codePoint, int strokeCount)
        {
            CodePoint = codePoint;
            StrokeCount = strokeCount;
        }

        public override string ToString()
        {
            return char.ConvertFromUtf32(CodePoint);
        }
    }

    public class Radkfile
    {
        private Dictionary<int, Tuple<int, List<int>>> entries = new Dictionary<int, Tuple<int, List<int>>>();

        private List<KeyValuePair<int, int>> radicals = new List<KeyValuePair<int, int>>();

        public IEnumerable<string> LookupMatching(IEnumerable<string> radicals)
        {
            bool first = true;
            IEnumerable<int> set = new List<int>();
            foreach(var radical in radicals)
            {
                if(first)
                {
                    set = new List<int>(entries[char.ConvertToUtf32(radical, 0)].Item2);
                    first = false;
                }
                else
                {
                    set = set.Intersect(entries[char.ConvertToUtf32(radical, 0)].Item2);
                }
            }
            return set.Select(cp => char.ConvertFromUtf32(cp)).ToList();
        }

        public IEnumerable<Radical> Radicals => radicals.Select(kvp => new Radical(kvp.Value, kvp.Key));

        public class Entry
        {
            public Radical Radical { get; }

            public IReadOnlyCollection<int> KanjiCodePoints { get; }

            public Entry(Radical radical, IReadOnlyCollection<int> kanjiCodePoints)
            {
                Radical = radical;
                KanjiCodePoints = kanjiCodePoints;
            }
        }

        public static IEnumerable<Entry> Parse(string path)
        {
            using (var reader = File.OpenText(path))
            {
                return Parse(reader);
            }
        }

        public static IEnumerable<Entry> Parse(TextReader reader)
        {
            string line;
            var radical = default(Radical);
            var result = new List<Entry>();
            List<int> kanji = null;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#"))
                    continue;
                if (line.StartsWith("$"))
                {
                    if(kanji != null)
                        result.Add(new Entry(radical, kanji));

                    kanji = new List<int>();
                    line = line.Remove(0, 2);
                    var components = line.Split(' ');
                    int codePoint = char.ConvertToUtf32(components[0].Trim(), 0);
                    int strokeCount = int.Parse(components[1]);
                    radical = new Radical(codePoint, strokeCount);
                }
                else
                {
                    if(kanji == null)
                        throw new InvalidDataException();

                    foreach (var codePoint in AsCodePoints(line.Trim()))
                    {
                        kanji.Add(codePoint);
                    }
                }
            }

            if (kanji?.Count != 0)
            {
                result.Add(new Entry(radical, kanji));
            }

            return result
                .GroupBy(e => e.Radical.CodePoint)
                .Select(g =>
                {
                    var codePoint = g.Key;
                    var strokeCount = g.First().Radical.StrokeCount;
                    var k = g.SelectMany(e => e.KanjiCodePoints).ToList();
                    return new Entry(new Radical(codePoint, strokeCount), k);
                });
        }

        private void Init(TextReader reader)
        {
            string line;
            List<int> current = null;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#"))
                    continue;
                if (line.StartsWith("$"))
                {
                    line = line.Remove(0, 2);
                    var components = line.Split(' ');
                    int radical = char.ConvertToUtf32(components[0].Trim(), 0);
                    int strokeCount = int.Parse(components[1]);
                    radicals.Add(new KeyValuePair<int, int>(strokeCount, radical));
                    current = GetOrAdd(entries, radical, _ => Tuple.Create(strokeCount, new List<int>())).Item2;
                }
                else
                {
                    if (current == null)
                        throw new InvalidDataException();

                    foreach (var codePoint in AsCodePoints(line.Trim()))
                    {
                        current.Add(codePoint);
                    }
                }
            }
            radicals = radicals.Distinct().OrderBy(p => p.Key).ThenBy(p => p.Value).ToList();
        }

        public Radkfile(string path, Encoding encoding)
        {
            using (var reader = new StreamReader(path, encoding))
            {
                Init(reader);
            }
        }

        public Radkfile(TextReader reader)
        {
            Init(reader);
        }

        private static TValue GetOrAdd<TKey, TValue>(IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> valueFactory)
        {
            if(dict.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                value = valueFactory(key);
                dict[key] = value;
                return value;
            }
        }

        private static IEnumerable<int> AsCodePoints(string s)
        {
            for (int i = 0; i < s.Length; ++i)
            {
                yield return char.ConvertToUtf32(s, i);
                if (char.IsHighSurrogate(s, i))
                    i++;
            }
        }
    }
}
