using System;
using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Utils;
using JDict;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class LanguageService : ILanguageService
    {
        public IEnumerable<IEnumerable<WordInfo>> BreakIntoSentences(string input)
        {
            if (input.Trim() == "")
                return Enumerable.Empty<IEnumerable<WordInfo>>();
            return input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Select(line =>
                {
                    return meCab.ParseToEntries(line)
                        .Where(a => a.IsRegular)
                        .Select(word => new WordInfo(
                            word.OriginalForm,
                            LookupWord(word.OriginalForm).DictionaryDefinition,
                            word.PartOfSpeech,
                            word.Reading));
                });
        }

        public void Dispose()
        {
            meCab.Dispose();
        }

        public string LookupRomaji(Kana kana)
        {
            return kanaProperties.LookupRomaji(kana.ToString());
        }

        public CodePoint LookupCharacter(string character, int position = 0)
        {
            return CodePoint.FromString(character, position);
        }

        public CodePoint LookupCharacter(int codePoint)
        {
            return CodePoint.FromInt(codePoint);
        }

        public WordInfo LookupWord(string word)
        {
            //var entry = dictionary.Lookup(word.Trim());
            //return new WordInfo(word, entry?.ToString());
            /*var entry = jdict.Lookup(word.Trim());
            string joined = null;
            if (entry != null)
            {
                joined = string.Join("\n\n", entry.Select(e => e.ToString()));
            }*/
            return new WordInfo(word, "");
        }

        private readonly IMeCab meCab;

        private readonly EasilyConfusedKana confused;

        private readonly Kradfile kradfile;

        private readonly Radkfile radkfile;

        private readonly KanjiDict kanjidict;

        private readonly KanaProperties kanaProperties;

        public LanguageService(
            IMeCab meCab,
            EasilyConfusedKana similar,
            Kradfile kradfile,
            Radkfile radkfile,
            KanjiDict kanjiDict,
            KanaProperties kanaProperties)
        {
            this.meCab = meCab;
            this.confused = similar;
            this.kradfile = kradfile;
            this.radkfile = radkfile;
            this.kanjidict = kanjiDict;
            this.kanaProperties = kanaProperties;
        }

        private IEnumerable<string> SplitWords(string input)
        {
            input = input.Trim();
            int start = 0;
            int end = 0;
            bool current = false;
            for (int i = 0; i < input.Length; ++i)
            {
                if (char.IsWhiteSpace(input[i]) == current)
                {
                    ++end;
                }
                else
                {
                    current = !current;
                    yield return input.Substring(start, end - start);
                    start = end;
                    ++end;
                }
            }
            if (start != end)
            {
                yield return input.Substring(start, end - start);
            }
        }

        public IEnumerable<CodePoint> LookupRelatedCharacters(CodePoint point)
        {
            return EnumerableExt.IntersperseSequencesWith(new[]
                {
                    kanaProperties.FindSimilar(point),
                    confused.FindSimilar(point)
                },
                null as CodePoint);
        }

        public IEnumerable<CodePoint> LookupRadicals(Kanji kanji)
        {
            return kradfile.LookupRadicals(kanji.ToString())?.Select(cp => CodePoint.FromString(cp));
        }

        public IEnumerable<CodePoint> LookupByRadicals(IEnumerable<CodePoint> radicals)
        {
            return radkfile.LookupMatching(radicals.Select(r => r.ToString())).OrderBy(r => kanjidict.Lookup(r).StrokeCount).Select(cp => CodePoint.FromString(cp, 0));
        }

        public IEnumerable<Radical> AllRadicals()
        {
            return radkfile.Radicals
                .Select(rad => (codePoint: CodePoint.FromInt(rad.CodePoint), strokeCount: rad.StrokeCount))
                .Select(p => new Radical(p.codePoint, p.strokeCount))
                .OrderBy(r => r.StrokeCount);
        }
    }
}