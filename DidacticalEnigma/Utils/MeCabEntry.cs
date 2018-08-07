using System.Collections.Generic;
using System.Linq;
using NMeCab;

namespace DidacticalEnigma.Utils
{
    public class MeCabEntry
    {
        public MeCabNode Node { get; }

        public bool IsRegular => !(Stat == MeCabNodeStat.Eos || Stat == MeCabNodeStat.Bos);

        public MeCabEntry(MeCabNode n)
        {
            Node = n;
            OriginalForm = Node.Surface;
            Stat = Node.Stat;
            if (!IsRegular)
                return;
            var features = Node.Feature.Split(',');
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
                    return DidacticalEnigma.Utils.PartOfSpeech.Noun;
                case "助詞":
                    return DidacticalEnigma.Utils.PartOfSpeech.Particle;
                case "動詞":
                    return DidacticalEnigma.Utils.PartOfSpeech.Verb;
                case "助動詞":
                    return DidacticalEnigma.Utils.PartOfSpeech.AuxiliaryVerb;
                case "記号":
                    return DidacticalEnigma.Utils.PartOfSpeech.Symbol;
                case "副詞":
                    return DidacticalEnigma.Utils.PartOfSpeech.Adverb;
                case "形容詞":
                    return DidacticalEnigma.Utils.PartOfSpeech.Adjective;
                case "接続詞":
                    return DidacticalEnigma.Utils.PartOfSpeech.Conjunction;
                case "連体詞":
                    // TOFIX: name
                    return DidacticalEnigma.Utils.PartOfSpeech.PreNounAdjectivalAdjective;
                case "フィラー":
                    return DidacticalEnigma.Utils.PartOfSpeech.Filler;
                case "感動詞":
                    return DidacticalEnigma.Utils.PartOfSpeech.Interjection;
                case "接頭詞":
                    return DidacticalEnigma.Utils.PartOfSpeech.Prefix;
                case "その他":
                    return DidacticalEnigma.Utils.PartOfSpeech.Other;
                default:
                    return DidacticalEnigma.Utils.PartOfSpeech.Unknown;
            }
        }

        public string OriginalForm { get; }

        public DidacticalEnigma.Utils.PartOfSpeech PartOfSpeech { get; }

        public IEnumerable<string> PartOfSpeechSections { get; }

        public MeCabNodeStat Stat { get; }

        public string ConjugatedForm { get; }

        public string Inflection { get; }

        public string Reading { get; }

        public string Pronunciation { get; }
    }
}