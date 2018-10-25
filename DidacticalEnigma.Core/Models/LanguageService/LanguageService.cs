using System;
using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Utils;
using JDict;
using Optional;

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
                    return morphologicalAnalyzer.ParseToEntries(line)
                        .Where(a => a.IsRegular)
                        .Select(word => new WordInfo(
                            word.SurfaceForm,
                            word.PartOfSpeech,
                            word.DictionaryForm,
                            word.IsIndependent,
                            word.PartOfSpeechInfo.Contains(PartOfSpeechInfo.Pronoun) ? Option.Some(EdictType.pn) : word.Type));
                });
        }

        public void Dispose()
        {
            morphologicalAnalyzer.Dispose();
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

        private readonly IMorphologicalAnalyzer<IEntry> morphologicalAnalyzer;

        private readonly EasilyConfusedKana confused;

        private readonly Kradfile kradfile;

        private readonly Radkfile radkfile;

        private readonly KanjiDict kanjidict;

        private readonly IKanaProperties kanaProperties;

        private readonly RadicalRemapper remapper;

        public LanguageService(
            IMorphologicalAnalyzer<IEntry> morphologicalAnalyzer,
            EasilyConfusedKana similar,
            Kradfile kradfile,
            Radkfile radkfile,
            KanjiDict kanjiDict,
            IKanaProperties kanaProperties,
            RadicalRemapper remapper)
        {
            this.morphologicalAnalyzer = morphologicalAnalyzer;
            this.confused = similar;
            this.kradfile = kradfile;
            this.radkfile = radkfile;
            this.kanjidict = kanjiDict;
            this.kanaProperties = kanaProperties;
            this.remapper = remapper;
        }

        public Option<IEnumerable<CodePoint>> LookupRadicals(Kanji kanji)
        {
            return remapper
                .LookupRadicals(kanji.ToString())
                .Map(radicals => radicals.Select(cp => CodePoint.FromString(cp)));
        }

        public IEnumerable<CodePoint> LookupByRadicals(IEnumerable<CodePoint> radicals)
        {
            return remapper.LookupKanji(radicals.Select(r => r.ToString()))
                .OrderBy(r => kanjidict.Lookup(r)
                    .Map(e => e.StrokeCount)
                    .ValueOr(int.MaxValue))
                .Select(cp => CodePoint.FromString(cp));
        }

        public IEnumerable<Radical> AllRadicals()
        {
            return remapper.Radicals
                .Select(rad => (codePoint: CodePoint.FromInt(rad.CodePoint), strokeCount: rad.StrokeCount))
                .Select(p => new Radical(p.codePoint, p.strokeCount))
                .OrderBy(r => r.StrokeCount);
        }
    }
}