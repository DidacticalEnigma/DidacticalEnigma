using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IRomaji
    {
        string ToRomaji(string input);
    }

    public class ModifiedHepburn : IRomaji
    {
        private readonly IMorphologicalAnalyzer<IEntry> mecab;
        private readonly KanaProperties props;

        public string ToRomaji(string input)
        {
            return string.Join(" ", mecab.ParseToEntries(input)
                .Where(entry => entry.IsRegular)
                .Select(entry => IndividualWord(props.ToHiragana(entry.Pronunciation))));
        }

        // this is broken for combo kana
        private string IndividualWord(string word)
        {
            return string.Join("", word.Select(c => props.LookupRomaji(c.ToString())));
        }

        public ModifiedHepburn(IMorphologicalAnalyzer<IEntry> mecab, KanaProperties props)
        {
            this.mecab = mecab;
            this.props = props;
        }
    }
}
