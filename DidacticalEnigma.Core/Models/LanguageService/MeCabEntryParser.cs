using System.Collections.Generic;
using System.Linq;
using JDict;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    internal static class MeCabEntryParser
    {
        internal static Option<EdictPartOfSpeech> TypeFromString(string s)
        {
            switch (s)
            {
                case "五段・ラ行":
                    return Option.None<EdictPartOfSpeech>();
                case "特殊・マス":
                    return Option.None<EdictPartOfSpeech>();
                case "特殊・タ":
                    return Option.None<EdictPartOfSpeech>();
                case "一段":
                    return Option.None<EdictPartOfSpeech>();
                case "サ変・スル":
                    return Option.Some(EdictPartOfSpeech.vs_i);
                case "特殊・ナイ":
                    return Option.None<EdictPartOfSpeech>();
                case "五段・サ行":
                    return Option.None<EdictPartOfSpeech>();
                case "形容詞・アウオ段":
                    return Option.None<EdictPartOfSpeech>();
                case "五段・カ行イ音便":
                    return Option.None<EdictPartOfSpeech>();
                case "五段・ワ行促音便":
                    return Option.None<EdictPartOfSpeech>();
                case "特殊・ダ":
                    return Option.Some(EdictPartOfSpeech.cop_da);
                case "五段・バ行":
                    return Option.None<EdictPartOfSpeech>();
                case "カ変・クル":
                    return Option.Some(EdictPartOfSpeech.vk);
                case "不変化型":
                    return Option.None<EdictPartOfSpeech>();
                case "五段・タ行":
                    return Option.None<EdictPartOfSpeech>();
                case "五段・マ行":
                    return Option.None<EdictPartOfSpeech>();
                case "五段・カ行促音便":
                    return Option.None<EdictPartOfSpeech>();
                case "特殊・デス":
                    return Option.None<EdictPartOfSpeech>();
                case "カ変・来ル":
                    return Option.None<EdictPartOfSpeech>();
                case "五段・ラ行特殊":
                    return Option.None<EdictPartOfSpeech>();
                case "特殊・タイ":
                    return Option.None<EdictPartOfSpeech>();
                case "形容詞・イ段":
                    return Option.None<EdictPartOfSpeech>();
                case "文語・ベシ":
                    return Option.None<EdictPartOfSpeech>();
                case "五段・ラ行アル":
                    return Option.None<EdictPartOfSpeech>();
                case "形容詞・イイ":
                    return Option.None<EdictPartOfSpeech>();
                case "五段・カ行促音便ユク":
                    return Option.None<EdictPartOfSpeech>();
                case "一段・クレル":
                    return Option.None<EdictPartOfSpeech>();
                case "五段・ガ行":
                    return Option.None<EdictPartOfSpeech>();
                case "下二・タ行":
                    return Option.None<EdictPartOfSpeech>();
                case "特殊・ヌ":
                    return Option.None<EdictPartOfSpeech>();
                case "文語・ナリ":
                    return Option.None<EdictPartOfSpeech>();
                case "五段・ナ行":
                    return Option.None<EdictPartOfSpeech>();
                case "サ変・－スル":
                    return Option.None<EdictPartOfSpeech>();
                case "文語・キ":
                    return Option.None<EdictPartOfSpeech>();
                case "一段・得ル":
                    return Option.None<EdictPartOfSpeech>();
                case "文語・リ":
                    return Option.None<EdictPartOfSpeech>();
                case "サ変・－ズル":
                    return Option.None<EdictPartOfSpeech>();
                case "特殊・ヤ":
                    return Option.None<EdictPartOfSpeech>();
                case "文語・ル":
                    return Option.None<EdictPartOfSpeech>();
                case "特殊・ジャ":
                    return Option.None<EdictPartOfSpeech>();
                case "文語・ゴトシ":
                    return Option.None<EdictPartOfSpeech>();
                case "ラ変":
                    return Option.None<EdictPartOfSpeech>();
                case "四段・ハ行":
                    return Option.None<EdictPartOfSpeech>();
                case "下二・カ行":
                    return Option.None<EdictPartOfSpeech>();
                case "上二・ダ行":
                    return Option.None<EdictPartOfSpeech>();
                case "下二・ガ行":
                    return Option.None<EdictPartOfSpeech>();
                case "四段・バ行":
                    return Option.None<EdictPartOfSpeech>();
                case "下二・マ行":
                    return Option.None<EdictPartOfSpeech>();
                case "五段・ワ行ウ音便":
                    return Option.None<EdictPartOfSpeech>();
                case "下二・ダ行":
                    return Option.None<EdictPartOfSpeech>();
                default:
                    return Option.None<EdictPartOfSpeech>();
            }
        }

        internal static bool? IsIndependentFromSections(IEnumerable<string> sections)
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

        internal static PartOfSpeech PartOfSpeechFromString(string s)
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
                case "代名詞":
                    return PartOfSpeech.Pronoun;
                default:
                    return PartOfSpeech.Unknown;
            }
        }

        internal static PartOfSpeechInfo PartOfSpeechInfoFromString(string arg)
        {
            switch(arg)
            {
                // general/universal/ordinary/average/common
                case "一般":
                    return PartOfSpeechInfo.Unknown;
                // binding particle (i.e. specifying an expression later in the sentence)/linking particle/connecting particle
                case "係助詞":
                    return PartOfSpeechInfo.Unknown;
                // number/amount
                case "数":
                    return PartOfSpeechInfo.Unknown;
                // suffix
                case "接尾":
                    return PartOfSpeechInfo.Unknown;
                // counters for various categories/counter suffix
                case "助数詞":
                    return PartOfSpeechInfo.Counter;
                // case-marking particle
                case "格助詞":
                    return PartOfSpeechInfo.Unknown;
                // independence/self-reliance
                case "自立":
                    return PartOfSpeechInfo.Unknown;
                // period/full stop
                case "句点":
                    return PartOfSpeechInfo.Unknown;
                // (???)
                case "助詞類接続":
                    return PartOfSpeechInfo.Unknown;
                // adverb, action of making something/-ification
                case "副詞化":
                    return PartOfSpeechInfo.Unknown;
                // conjuctive particle
                case "接続助詞":
                    return PartOfSpeechInfo.Unknown;
                // (???)
                case "サ変接続":
                    return PartOfSpeechInfo.Unknown;
                // adverbial particle
                case "副助詞":
                    return PartOfSpeechInfo.Unknown;
                // not independent
                case "非自立":
                    return PartOfSpeechInfo.Unknown;
                // sentence-ending particle (yo, ne, kashi, etc.)
                case "終助詞":
                    return PartOfSpeechInfo.SentenceEndingParticle;
                // pronoun
                case "代名詞":
                    return PartOfSpeechInfo.Pronoun;
                // parallel marker (particle used to join two or more words, i.e. "to", "ya")
                case "並立助詞":
                    return PartOfSpeechInfo.Unknown;
                // open parentheses/brackets
                case "括弧開":
                    return PartOfSpeechInfo.Unknown;
                // close parentheses/brackets
                case "括弧閉":
                    return PartOfSpeechInfo.Unknown;
                // quotation/citation/reference
                case "引用":
                    return PartOfSpeechInfo.Unknown;
                // auxiliary verb's stem/root of a word (???)
                case "助動詞語幹":
                    return PartOfSpeechInfo.Unknown;
                // proper noun
                case "固有名詞":
                    return PartOfSpeechInfo.Unknown;
                // person's name
                case "人名":
                    return PartOfSpeechInfo.Unknown;
                // name
                case "名":
                    return PartOfSpeechInfo.Unknown;
                // pre-noun adjectival/adnominal adjective, action of making something/-ification
                case "連体化":
                    return PartOfSpeechInfo.Unknown;
                // adjectival noun (Japanese)/quasi-adjective/nominal adjective/na-, taru-, nari- and tari-adjective, stem/root of a word
                case "形容動詞語幹":
                    return PartOfSpeechInfo.Unknown;
                // adverb, MAYBE
                case "副詞可能":
                    return PartOfSpeechInfo.Unknown;
                // comma
                case "読点":
                    return PartOfSpeechInfo.Unknown;
                // adverbial particle OR parallel marker (particle used to join two or more words, i.e. "to", "ya") OR sentence-ending particle (yo, ne, kashi, etc.)
                case "副助詞／並立助詞／終助詞":
                    return PartOfSpeechInfo.Unknown;
                // (???)
                case "組織":
                    return PartOfSpeechInfo.Unknown;
                // (???)
                case "ナイ形容詞語幹":
                    return PartOfSpeechInfo.Unknown;
                // area/region
                case "地域":
                    return PartOfSpeechInfo.Unknown;
                // compound word/phrase/collocation
                case "連語":
                    return PartOfSpeechInfo.Unknown;
                // noun, connection (???)
                case "名詞接続":
                    return PartOfSpeechInfo.Unknown;
                // surname/family name
                case "姓":
                    return PartOfSpeechInfo.Unknown;
                // country/state/region (???)
                case "国":
                    return PartOfSpeechInfo.Unknown;
                // special/particular/peculiar/unique
                case "特殊":
                    return PartOfSpeechInfo.Unknown;
                // number/amount, connection
                case "数接続":
                    return PartOfSpeechInfo.Unknown;
                // alphabet
                case "アルファベット":
                    return PartOfSpeechInfo.Unknown;
                // blank space/vacuum/space/null (NUL)
                case "空白":
                    return PartOfSpeechInfo.Unknown;
                // interjection
                case "間投":
                    return PartOfSpeechInfo.Unknown;
                // ???
                case "形容詞接続":
                    return PartOfSpeechInfo.Unknown;
                // conjunction, LIKE
                case "接続詞的":
                    return PartOfSpeechInfo.Unknown;
                // verb conjunction
                case "動詞接続":
                    return PartOfSpeechInfo.Unknown;
                // verb, not independent, LIKE
                case "動詞非自立的":
                    return PartOfSpeechInfo.Unknown;
                // quoted (character) string
                case "引用文字列":
                    return PartOfSpeechInfo.Unknown;
                // place name (???)
                case "地名":
                    return PartOfSpeechInfo.Unknown;
                // common noun
                case "普通名詞":
                    return PartOfSpeechInfo.Unknown;
                // counters for various categories/counter suffix, MAYBE (???)
                case "助数詞可能":
                    return PartOfSpeechInfo.Unknown;
                // numeral
                case "数詞":
                    return PartOfSpeechInfo.Unknown;
                // noun, LIKE (???)
                case "名詞的":
                    return PartOfSpeechInfo.Unknown;
                // not independent, MAYBE
                case "非自立可能":
                    return PartOfSpeechInfo.Unknown;
                // irregular conjugation (inflection, declension) of s-stem verbs/conjugation of the verb "suru", MAYBE
                case "サ変可能":
                    return PartOfSpeechInfo.Unknown;
                // 
                case "準体助詞":
                    return PartOfSpeechInfo.Unknown;
                // (???)
                case "形状詞可能":
                    return PartOfSpeechInfo.Unknown;
                // (???)
                case "サ変形状詞可能":
                    return PartOfSpeechInfo.Unknown;
                // filler
                case "フィラー":
                    return PartOfSpeechInfo.Unknown;
                // letter (of alphabet)/character
                case "文字":
                    return PartOfSpeechInfo.Unknown;
                // i-adjective, LIKE
                case "形容詞的":
                    return PartOfSpeechInfo.Unknown;
                // (???), LIKE
                case "形状詞的":
                    return PartOfSpeechInfo.Unknown;
                // verb, LIKE
                case "動詞的":
                    return PartOfSpeechInfo.Unknown;
                // (???)
                case "タリ":
                    return PartOfSpeechInfo.Unknown;
                default:
                    return PartOfSpeechInfo.Unknown;
            }
        }

        internal static string OrNull(string[] input, int index)
        {
            return index >= input.Length || input[index] == "*" ? null : input[index];
        }

        public static IEnumerable<PartOfSpeechInfo> GetPartOfSpeechInfo(this IEntry entry) =>
            entry.PartOfSpeechSections.Select(MeCabEntryParser.PartOfSpeechInfoFromString);
    }
}