using System.Collections.Generic;
using JDict;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    internal static class MeCabEntryParser
    {
        public static Option<EdictType> TypeFromString(string s)
        {
            switch (s)
            {
                case "五段・ラ行":
                    return Option.None<EdictType>();
                case "特殊・マス":
                    return Option.None<EdictType>();
                case "特殊・タ":
                    return Option.None<EdictType>();
                case "一段":
                    return Option.None<EdictType>();
                case "サ変・スル":
                    return Option.Some(EdictType.vs_i);
                case "特殊・ナイ":
                    return Option.None<EdictType>();
                case "五段・サ行":
                    return Option.None<EdictType>();
                case "形容詞・アウオ段":
                    return Option.None<EdictType>();
                case "五段・カ行イ音便":
                    return Option.None<EdictType>();
                case "五段・ワ行促音便":
                    return Option.None<EdictType>();
                case "特殊・ダ":
                    return Option.Some(EdictType.cop_da);
                case "五段・バ行":
                    return Option.None<EdictType>();
                case "カ変・クル":
                    return Option.Some(EdictType.vk);
                case "不変化型":
                    return Option.None<EdictType>();
                case "五段・タ行":
                    return Option.None<EdictType>();
                case "五段・マ行":
                    return Option.None<EdictType>();
                case "五段・カ行促音便":
                    return Option.None<EdictType>();
                case "特殊・デス":
                    return Option.None<EdictType>();
                case "カ変・来ル":
                    return Option.None<EdictType>();
                case "五段・ラ行特殊":
                    return Option.None<EdictType>();
                case "特殊・タイ":
                    return Option.None<EdictType>();
                case "形容詞・イ段":
                    return Option.None<EdictType>();
                case "文語・ベシ":
                    return Option.None<EdictType>();
                case "五段・ラ行アル":
                    return Option.None<EdictType>();
                case "形容詞・イイ":
                    return Option.None<EdictType>();
                case "五段・カ行促音便ユク":
                    return Option.None<EdictType>();
                case "一段・クレル":
                    return Option.None<EdictType>();
                case "五段・ガ行":
                    return Option.None<EdictType>();
                case "下二・タ行":
                    return Option.None<EdictType>();
                case "特殊・ヌ":
                    return Option.None<EdictType>();
                case "文語・ナリ":
                    return Option.None<EdictType>();
                case "五段・ナ行":
                    return Option.None<EdictType>();
                case "サ変・－スル":
                    return Option.None<EdictType>();
                case "文語・キ":
                    return Option.None<EdictType>();
                case "一段・得ル":
                    return Option.None<EdictType>();
                case "文語・リ":
                    return Option.None<EdictType>();
                case "サ変・－ズル":
                    return Option.None<EdictType>();
                case "特殊・ヤ":
                    return Option.None<EdictType>();
                case "文語・ル":
                    return Option.None<EdictType>();
                case "特殊・ジャ":
                    return Option.None<EdictType>();
                case "文語・ゴトシ":
                    return Option.None<EdictType>();
                case "ラ変":
                    return Option.None<EdictType>();
                case "四段・ハ行":
                    return Option.None<EdictType>();
                case "下二・カ行":
                    return Option.None<EdictType>();
                case "上二・ダ行":
                    return Option.None<EdictType>();
                case "下二・ガ行":
                    return Option.None<EdictType>();
                case "四段・バ行":
                    return Option.None<EdictType>();
                case "下二・マ行":
                    return Option.None<EdictType>();
                case "五段・ワ行ウ音便":
                    return Option.None<EdictType>();
                case "下二・ダ行":
                    return Option.None<EdictType>();
                default:
                    return Option.None<EdictType>();
            }
        }

        public static bool? IsIndependentFromSections(IEnumerable<string> sections)
        {
            foreach(var s in sections)
            {
                switch(s)
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

        public static PartOfSpeech PartOfSpeechFromString(string s)
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
                    return PartOfSpeech.PreNounAdjectival;
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

        public static PartOfSpeechInfo PartOfSpeechInfoFromString(string arg)
        {
            switch(arg)
            {
                case "一般":
                    return PartOfSpeechInfo.Unknown;
                case "係助詞":
                    return PartOfSpeechInfo.Unknown;
                case "数":
                    return PartOfSpeechInfo.Unknown;
                case "接尾":
                    return PartOfSpeechInfo.Unknown;
                case "助数詞":
                    return PartOfSpeechInfo.Unknown;
                case "格助詞":
                    return PartOfSpeechInfo.Unknown;
                case "自立":
                    return PartOfSpeechInfo.Unknown;
                case "句点":
                    return PartOfSpeechInfo.Unknown;
                case "助詞類接続":
                    return PartOfSpeechInfo.Unknown;
                case "副詞化":
                    return PartOfSpeechInfo.Unknown;
                case "接続助詞":
                    return PartOfSpeechInfo.Unknown;
                case "サ変接続":
                    return PartOfSpeechInfo.Unknown;
                case "副助詞":
                    return PartOfSpeechInfo.Unknown;
                case "非自立":
                    return PartOfSpeechInfo.Unknown;
                case "終助詞":
                    return PartOfSpeechInfo.Unknown;
                case "代名詞":
                    return PartOfSpeechInfo.Pronoun;
                case "並立助詞":
                    return PartOfSpeechInfo.Unknown;
                case "括弧開":
                    return PartOfSpeechInfo.Unknown;
                case "括弧閉":
                    return PartOfSpeechInfo.Unknown;
                case "引用":
                    return PartOfSpeechInfo.Unknown;
                case "助動詞語幹":
                    return PartOfSpeechInfo.Unknown;
                case "固有名詞":
                    return PartOfSpeechInfo.Unknown;
                case "人名":
                    return PartOfSpeechInfo.Unknown;
                case "名":
                    return PartOfSpeechInfo.Unknown;
                case "連体化":
                    return PartOfSpeechInfo.Unknown;
                case "形容動詞語幹":
                    return PartOfSpeechInfo.Unknown;
                case "副詞可能":
                    return PartOfSpeechInfo.Unknown;
                case "読点":
                    return PartOfSpeechInfo.Unknown;
                case "副助詞／並立助詞／終助詞":
                    return PartOfSpeechInfo.Unknown;
                case "組織":
                    return PartOfSpeechInfo.Unknown;
                case "ナイ形容詞語幹":
                    return PartOfSpeechInfo.Unknown;
                case "地域":
                    return PartOfSpeechInfo.Unknown;
                case "連語":
                    return PartOfSpeechInfo.Unknown;
                case "名詞接続":
                    return PartOfSpeechInfo.Unknown;
                case "姓":
                    return PartOfSpeechInfo.Unknown;
                case "国":
                    return PartOfSpeechInfo.Unknown;
                case "特殊":
                    return PartOfSpeechInfo.Unknown;
                case "数接続":
                    return PartOfSpeechInfo.Unknown;
                case "アルファベット":
                    return PartOfSpeechInfo.Unknown;
                case "空白":
                    return PartOfSpeechInfo.Unknown;
                case "間投":
                    return PartOfSpeechInfo.Unknown;
                case "形容詞接続":
                    return PartOfSpeechInfo.Unknown;
                case "接続詞的":
                    return PartOfSpeechInfo.Unknown;
                case "動詞接続":
                    return PartOfSpeechInfo.Unknown;
                case "動詞非自立的":
                    return PartOfSpeechInfo.Unknown;
                case "引用文字列":
                    return PartOfSpeechInfo.Unknown;
                default:
                    return PartOfSpeechInfo.Unknown;
            }
        }

        public static string OrNull(string[] input, int index)
        {
            return index >= input.Length || input[index] == "*" ? null : input[index];
        }
    }
}