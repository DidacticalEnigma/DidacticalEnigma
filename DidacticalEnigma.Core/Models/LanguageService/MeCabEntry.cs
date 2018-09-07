using System.Collections.Generic;
using System.Linq;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class MeCabEntry
    {
        public bool IsRegular { get; }

        public MeCabEntry(string originalForm, string feature, bool isRegular)
        {
            OriginalForm = originalForm;
            IsRegular = isRegular;
            if(!IsRegular)
                return;
            var features = feature.Split(',');
            PartOfSpeech = PartOfSpeechFromString(OrNull(features, 0));
            ConjugatedForm = OrNull(features, 4);
            Inflection = OrNull(features, 5);
            Reading = OrNull(features, 6);
            Pronunciation = OrNull(features, 7);
            PartOfSpeechSections = features
                .Skip(1)
                .Take(3)
                .Where(f => f != "*")
                .ToList()
                .AsReadOnly();

            string OrNull(string[] input, int index)
            {
                return index >= input.Length || input[index] == "*" ? null : input[index];
            }
        }

        private static PartOfSpeech PartOfSpeechFromString(string s)
        {
            switch(s)
            {
                case "名詞":
                return PartOfSpeech.Noun;
                case "助詞":
                return PartOfSpeech.Particle;
                case "動詞":
                return PartOfSpeech.Verb;
                case "助動詞":
                return PartOfSpeech.AuxiliaryVerb;
                case "記号":
                return PartOfSpeech.Symbol;
                case "副詞":
                return PartOfSpeech.Adverb;
                case "形容詞":
                return PartOfSpeech.Adjective;
                case "接続詞":
                return PartOfSpeech.Conjunction;
                case "連体詞":
                // TOFIX: name
                return PartOfSpeech.PreNounAdjectivalAdjective;
                case "フィラー":
                return PartOfSpeech.Filler;
                case "感動詞":
                return PartOfSpeech.Interjection;
                case "接頭詞":
                return PartOfSpeech.Prefix;
                case "その他":
                return PartOfSpeech.Other;
                default:
                return PartOfSpeech.Unknown;
            }
        }

        public string OriginalForm { get; }

        public PartOfSpeech PartOfSpeech { get; }

        public IEnumerable<string> PartOfSpeechSections { get; }

        public string ConjugatedForm { get; }

        public string Inflection { get; }

        public string Reading { get; }

        public string Pronunciation { get; }
    }
}