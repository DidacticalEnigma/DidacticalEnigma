using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JDict
{
    public class EDict
    {
        private Dictionary<string, EDictEntry> dict;

        public EDictEntry Lookup(string key)
        {
            dict.TryGetValue(key, out var value);
            return value;
        }

        public EDictEntry this[string key] => dict[key];

        // Converting
        // cat edict2 | iconv -t utf-8 -f EUC-JP > edict2_utf8
        public EDict(string path, Encoding encoding)
        {
            var entries = File.ReadLines(path, encoding)
                .Skip(1) // skip the header
                .Select(EDictEntry.Parse)
                .SelectMany(e => e.Kanji.Select(k => new KeyValuePair<string, EDictEntry>(k, e)));
            // ignore multiple mappings
            // TODO: see what can be done about that
            dict = new Dictionary<string, EDictEntry>();
            foreach (var entry in entries)
            {
                dict[entry.Key] = entry.Value;
            }
            //一くさり;一齣;一闋 [ひとくさり] /(n) passage in a discourse/one section/one scene/EntL1167580X/
            //１コマ;一コマ;１こま;一こま;一齣;一駒(iK) [ひとコマ(１コマ,一コマ);ひとこま(１こま,一こま,一齣,一駒)] /(n) (1) one scene/one frame/one shot/one exposure/(2) one cell/one panel (comic)/EntL1162000X/
        }
    }

    // KANJI-1;KANJI-2 [KANA-1;KANA-2] /(general information) (see xxxx) gloss/gloss/.../
    // 収集(P);蒐集;拾集;収輯 [しゅうしゅう] /(n,vs) gathering up/collection/accumulation/(P)/EntL1594720X/
    public class EDictEntry
    {
        public string SequenceNumber { get; }

        public IEnumerable<string> Readings { get; }

        public IEnumerable<string> Kanji { get; }

        public IEnumerable<EDictSense> Senses { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            bool first;
            {
                first = true;
                foreach (var kanji in Kanji)
                {
                    if (!first)
                        sb.Append(";  ");
                    first = false;
                    sb.Append(kanji);
                }
                sb.AppendLine();
            }
            {
                first = true;
                foreach (var reading in Readings)
                {
                    if (!first)
                        sb.Append(";  ");
                    first = false;
                    sb.Append(reading);
                }
                sb.AppendLine();
            }
            {
                foreach (var sense in Senses)
                {
                    sb.Append(sense);
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        public EDictEntry(
            string sequenceNumber,
            IEnumerable<string> readings,
            IEnumerable<string> kanji,
            IEnumerable<EDictSense> senses)
        {
            SequenceNumber = sequenceNumber;
            Readings = readings.ToList();
            Kanji = kanji.ToList();
            Senses = senses.ToList();
        }

        public static EDictEntry Parse(string line)
        {
            var e = line.Split('/');
            var sequenceNumber = e[e.Length - 2];
            var kanjiAndKana = e[0].Trim();
            int endingIndex = kanjiAndKana.Length;
            var kana = Enumerable.Empty<string>();
            if (kanjiAndKana.Last() == ']')
            {
                endingIndex = kanjiAndKana.LastIndexOf('[');
                if (endingIndex == -1)
                    endingIndex = kanjiAndKana.Length;
                kana = SubstringFromTo(kanjiAndKana, endingIndex + 1, kanjiAndKana.Length - 1).Split(';');
            }
            var kanji = SubstringFromTo(kanjiAndKana, 0, endingIndex).Trim().Split(';');
            var senses = e
                .Skip(1)
                .Take(e.Length - 3)
                .Select(s => new EDictSense(s));
            return new EDictEntry(sequenceNumber, kana, kanji, senses);

            string SubstringFromTo(string s, int begin, int end)
            {
                return s.Substring(begin, end - begin);
            }
        }
    }

    public class EDictSense
    {
        private string text;
        public EDictSense(string text)
        {
            this.text = text;
        }

        public override string ToString()
        {
            return text;
        }
    }
}
