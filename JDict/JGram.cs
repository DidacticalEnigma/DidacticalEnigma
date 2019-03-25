using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JDict
{
    public class JGram
    {
        public class Entry
        {
            public long Id { get; }

            public string Key { get; }

            public string Reading { get; }

            public string Romaji { get; }

            public string Translation { get; }

            public string Example { get; }

            public Entry(long id, string key, string reading, string romaji, string translation, string example)
            {
                Id = id;
                Key = key;
                Reading = reading;
                Romaji = romaji;
                Translation = translation;
                Example = example;
            }
        }


        public static IEnumerable<Entry> Parse(string path, Encoding encoding)
        {
            using (var reader = new StreamReader(path, encoding))
            {
                foreach (var entry in Parse(reader))
                {
                    yield return entry;
                }
            }
        }

        public static IEnumerable<Entry> Parse(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if(line.StartsWith("？") || string.IsNullOrWhiteSpace(line))
                    continue;

                long id = 0;
                string key = null;
                string reading = null;
                string romaji = null;
                string translation = null;
                string example = null;
                var components = line.Split('\t');
                foreach (var component in components.Select(c => c.Trim()))
                {
                    string idStart = "jgid=";
                    if (component == "")
                    {
                        continue;
                    }
                    if (component.StartsWith(idStart))
                    {
                        long.TryParse(component.Remove(0, idStart.Length), out id);
                    }
                    else if (component.StartsWith("/"))
                    {
                        translation = component.TrimStart('/').TrimEnd('/');
                    }
                    else if(component.StartsWith("\""))
                    {
                        example = component.TrimStart('\"').TrimEnd('\"');
                    }
                    else if(reading != null && component.StartsWith("["))
                    {
                        romaji = component.TrimStart('[').TrimEnd(']');
                    }
                    else if(component.StartsWith("["))
                    {
                        reading = component.TrimStart('[').TrimEnd(']');
                    }
                    else
                    {
                        key = component;
                    }
                }
                yield return new Entry(id, key, reading, romaji, translation, example);
            }
        }
    }
}
