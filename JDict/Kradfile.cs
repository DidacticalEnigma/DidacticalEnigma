using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Optional;
using Optional.Collections;

namespace JDict
{
    public class Kradfile
    {
        private Dictionary<string, IEnumerable<string>> entries = new Dictionary<string, IEnumerable<string>>();

        // returns decomposition of a kanji into radicals
        // returns null when not a kanji
        public Option<IEnumerable<string>> LookupRadicals(string kanji)
        {
            return entries.GetValueOrNone(kanji);
        }

        public IEnumerable<KeyValuePair<string, IEnumerable<string>>> AllRadicals()
        {
            foreach(var entry in entries)
            {
                yield return entry;
            }
        }

        private void Init(TextReader reader)
        {
            string line;
            while((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#", StringComparison.Ordinal))
                    continue;
                var parts = line.Split(':');
                var kanji = parts[0].Trim();
                var radicals = parts[1].Trim().Split(' ');
                entries.Add(kanji, radicals);
            }
        }

        public class Entry
        {
            public int Kanji { get; }

            public IReadOnlyCollection<int> Radicals { get; }

            public Entry(int kanji, IReadOnlyCollection<int> radicals)
            {
                Kanji = kanji;
                Radicals = radicals;
            }
        }

        public static IEnumerable<Entry> Parse(TextReader reader)
        {
            string line;
            var result = new List<Entry>();
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#", StringComparison.Ordinal))
                    continue;
                var parts = line.Split(':');
                var kanji = char.ConvertToUtf32(parts[0].Trim(), 0);
                var radicals = parts[1]
                    .Trim()
                    .Split(' ')
                    .Select(p => char.ConvertToUtf32(p, 0))
                    .ToList();
                result.Add(new Entry(kanji, radicals));
            }

            return result;
        }

        public Kradfile(string path, Encoding encoding)
        {
            using (var reader = new StreamReader(path, encoding))
            {
                Init(reader);
            }
        }

        public Kradfile(TextReader reader)
        {
            Init(reader);
        }
    }
}
