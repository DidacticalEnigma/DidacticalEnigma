using System;
using System.Collections.Generic;
using System.Linq;
using JDict;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class MeCabEntry
    {
        public bool IsRegular { get; }

        public MeCabEntry(string originalForm, Func<string> feature, bool isRegular)
        {
            OriginalForm = originalForm;
            IsRegular = isRegular;
            if(!IsRegular)
                return;
            var features = feature().Split(',');
            PartOfSpeech = PartOfSpeechFromString(OrNull(features, 0));
            ConjugatedForm = OrNull(features, 4);
            Type = TypeFromString(ConjugatedForm);
            Inflection = OrNull(features, 5);
            Reading = OrNull(features, 6);
            Pronunciation = OrNull(features, 7);
            PartOfSpeechSections = features
                .Skip(1)
                .Take(3)
                .Where(f => f != "*")
                .ToList()
                .AsReadOnly();
            IsIndependent = IsIndependentFromSections(PartOfSpeechSections);

            string OrNull(string[] input, int index)
            {
                return index >= input.Length || input[index] == "*" ? null : input[index];
            }
        }

        private static EdictType? TypeFromString(string s)
        {
            switch(s)
            {
                case "五段・ラ行":
                    return null;
                case "特殊・マス":
                    return null;
                case "特殊・タ":
                    return null;
                case "一段":
                    return null;
                case "サ変・スル":
                    return EdictType.vs_i;
                case "特殊・ナイ":
                    return null;
                case "五段・サ行":
                    return null;
                case "形容詞・アウオ段":
                    return null;
                case "五段・カ行イ音便":
                    return null;
                case "五段・ワ行促音便":
                    return null;
                case "特殊・ダ":
                    return null;
                case "五段・バ行":
                    return null;
                case "カ変・クル":
                    return null;
                case "不変化型":
                    return null;
                case "五段・タ行":
                    return null;
                case "五段・マ行":
                    return null;
                case "五段・カ行促音便":
                    return null;
                case "特殊・デス":
                    return null;
                case "カ変・来ル":
                    return null;
                case "五段・ラ行特殊":
                    return null;
                case "特殊・タイ":
                    return null;
                case "形容詞・イ段":
                    return null;
                case "文語・ベシ":
                    return null;
                case "五段・ラ行アル":
                    return null;
                case "形容詞・イイ":
                    return null;
                case "五段・カ行促音便ユク":
                    return null;
                case "一段・クレル":
                    return null;
                case "五段・ガ行":
                    return null;
                case "下二・タ行":
                    return null;
                case "特殊・ヌ":
                    return null;
                case "文語・ナリ":
                    return null;
                case "五段・ナ行":
                    return null;
                case "サ変・－スル":
                    return null;
                case "文語・キ":
                    return null;
                case "一段・得ル":
                    return null;
                case "文語・リ":
                    return null;
                case "サ変・－ズル":
                    return null;
                case "特殊・ヤ":
                    return null;
                case "文語・ル":
                    return null;
                case "特殊・ジャ":
                    return null;
                case "文語・ゴトシ":
                    return null;
                case "ラ変":
                    return null;
                case "四段・ハ行":
                    return null;
                case "下二・カ行":
                    return null;
                case "上二・ダ行":
                    return null;
                case "下二・ガ行":
                    return null;
                case "四段・バ行":
                    return null;
                case "下二・マ行":
                    return null;
                case "五段・ワ行ウ音便":
                    return null;
                case "下二・ダ行":
                    return null;
                default:
                    return null;
            }
        }

        private static bool? IsIndependentFromSections(IEnumerable<string> sections)
        {
            foreach (var s in sections)
            {
                switch (s)
                {
                    case "非自立":
                        return false;
                    case "自立":
                        return true;
                    default:
                        continue;
                }
            }

            return null;
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

        public EdictType? Type { get; }

        public string OriginalForm { get; }

        public PartOfSpeech PartOfSpeech { get; }

        public IEnumerable<string> PartOfSpeechSections { get; }

        public string ConjugatedForm { get; }

        public string Inflection { get; }

        public string Reading { get; }

        public string Pronunciation { get; }

        public bool? IsIndependent { get; }
    }
}