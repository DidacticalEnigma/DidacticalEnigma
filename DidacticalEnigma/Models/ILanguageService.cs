using DidacticalEnigma.Models;
using JDict;
using NMeCab;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DidacticalEnigma
{
    public interface ILanguageService : IDisposable
    {
        IEnumerable<CodePoint> LookupRelatedCharacters(CodePoint point);

        CodePoint LookupCharacter(string s, int position = 0);
        WordInfo LookupWord(string word);

        IEnumerable<IEnumerable<WordInfo>> BreakIntoSentences(string input);
    }

    // composite 
    class LanguageService : ILanguageService
    {
        public IEnumerable<IEnumerable<WordInfo>> BreakIntoSentences(string input)
        {
            if (input.Trim() == "")
                return Enumerable.Empty<IEnumerable<WordInfo>>();
            var parsed = mecab.Parse(input);
            var output = string.Join(
                "",
                SplitWords(parsed)
                    .Select(word => word.Contains("\r") || word.Contains("\n") ? "\n" : word));
            return output
                .Split('\n')
                .Select(line => line.Split().Select(word => new WordInfo(word, LookupWord(word)?.ToString() ?? "")));
        }

        public void Dispose()
        {
            mecab.Dispose();
        }

        public CodePoint LookupCharacter(string character, int position = 0)
        {
            return CodePoint.FromString(character, position);
        }

        public WordInfo LookupWord(string word)
        {
            var entry = dictionary.Lookup(word.Trim());
            return new WordInfo(word, entry?.ToString());
        }

        private readonly MeCabTagger mecab;

        private readonly EDict dictionary;

        private readonly SimilarKana similar;

        public LanguageService(MeCabParam mecabParam, EDict dictionary, SimilarKana similar)
        {
            mecabParam.LatticeLevel = MeCabLatticeLevel.Zero;
            mecabParam.OutputFormatType = "wakati";
            mecabParam.AllMorphs = false;
            mecabParam.Partial = false;
            this.mecab = MeCabTagger.Create(mecabParam);
            this.dictionary = dictionary;
            this.similar = similar;
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
            var oppositeSized = point is Kana k && k.HasOppositeSizedVersion
                ? Enumerable.Repeat(CodePoint.FromInt(k.OppositeSizedVersion), 1)
                : Enumerable.Empty<CodePoint>();
            return oppositeSized
                .Concat(similar.FindSimilar(point) ?? Enumerable.Empty<CodePoint>());
        }
    }
}
