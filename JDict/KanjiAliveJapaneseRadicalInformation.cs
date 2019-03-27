using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JDict
{
    public class KanjiAliveJapaneseRadicalInformation
    {
        public class Entry
        {
            public int? StrokeCount { get; }

            public string Literal { get; }

            public IReadOnlyCollection<string> Meanings { get; }

            public IReadOnlyCollection<string> JapaneseReadings { get; }

            public IReadOnlyCollection<string> RomajiReadings { get; }

            public IReadOnlyCollection<string> JapanesePositions { get; }

            public IReadOnlyCollection<string> RomajiPositions { get; }

            public bool Important { get; }

            public Entry(
                int? strokeCount,
                string literal,
                IReadOnlyCollection<string> meanings,
                IReadOnlyCollection<string> japaneseReadings,
                IReadOnlyCollection<string> romajiReadings,
                IReadOnlyCollection<string> japanesePositions,
                IReadOnlyCollection<string> romajiPositions,
                bool important)
            {
                StrokeCount = strokeCount;
                Literal = literal;
                Meanings = meanings;
                JapaneseReadings = japaneseReadings;
                RomajiReadings = romajiReadings;
                JapanesePositions = japanesePositions;
                RomajiPositions = romajiPositions;
                Important = important;
            }
        }

        public static IEnumerable<KanjiAliveJapaneseRadicalInformation.Entry> Parse(string path)
        {
            using (var reader = File.OpenText(path))
            {
                foreach (var entry in Parse(reader))
                {
                    yield return entry;
                }
            }
        }

        public static IEnumerable<KanjiAliveJapaneseRadicalInformation.Entry> Parse(TextReader reader)
        {
            string line;
            bool firstLine = true;

            int strokeIndex = -1;
            int radicalIndex = -1;
            int encodingIndex = -1;
            int meaningIndex = -1;
            int readingJapaneseIndex = -1;
            int readingRomajiIndex = -1;
            int positionJapaneseIndex = -1;
            int positionRomajiIndex = -1;
            int importanceIndex = -1;

            while ((line = reader.ReadLine()) != null)
            {
                var components = line.Split(';');
                if (firstLine)
                {
                    for (int i = 0; i < components.Length; ++i)
                    {
                        switch (components[i])
                        {
                            case "Strokes":
                                strokeIndex = i;
                                break;
                            case "Radical":
                                radicalIndex = i;
                                break;
                            case "Encoding":
                                encodingIndex = i;
                                break;
                            case "Meaning":
                                meaningIndex = i;
                                break;
                            case "Reading-J":
                                readingJapaneseIndex = i;
                                break;
                            case "Reading-R":
                                readingRomajiIndex = i;
                                break;
                            case "Position-J":
                                positionJapaneseIndex = i;
                                break;
                            case "Position-R":
                                positionRomajiIndex = i;
                                break;
                            case "Importance":
                                importanceIndex = i;
                                break;
                        }
                    }

                    firstLine = false;
                }
                else
                {
                    yield return new KanjiAliveJapaneseRadicalInformation.Entry(
                        int.TryParse(components[strokeIndex], out var strokeCount) ? strokeCount : null as int?,
                        components[radicalIndex],
                        components[meaningIndex].Split(',').Select(c => c.Trim()).ToList(),
                        components[readingJapaneseIndex].Split(',').Select(c => c.Trim()).ToList(),
                        components[readingRomajiIndex].Split(',').Select(c => c.Trim()).ToList(),
                        components[positionJapaneseIndex].Split(',').Select(c => c.Trim()).ToList(),
                        components[positionRomajiIndex].Split(',').Select(c => c.Trim()).ToList(),
                        components[importanceIndex] == "Important");
                }
            }
        }
    }
}
