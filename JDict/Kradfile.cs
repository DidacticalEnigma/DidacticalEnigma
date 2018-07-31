using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JDict
{
    public class Kradfile
    {
        private Dictionary<string, IEnumerable<string>> entries = new Dictionary<string, IEnumerable<string>>();

        // returns decomposition of a kanji into radicals
        // returns null when not a kanji
        public IEnumerable<string> LookupRadicals(string kanji)
        {
            entries.TryGetValue(kanji, out var entry);
            return entry;
        }

        private void Init(TextReader reader)
        {
            string line;
            while((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#"))
                    continue;
                var parts = line.Split(':');
                var kanji = parts[0].Trim();
                var radicals = parts[1].Trim().Split(' ');
                entries.Add(kanji, radicals);
            }
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
