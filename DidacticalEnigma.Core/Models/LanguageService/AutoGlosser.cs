using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.Project;
using DidacticalEnigma.Core.Utils;
using JDict;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class AutoGlosser
    {
        private readonly ILanguageService lang;
        private readonly JMDict dict;

        private static readonly IReadOnlyDictionary<PartOfSpeech, EdictType> mapping = new Dictionary<PartOfSpeech, EdictType>
        {
            { PartOfSpeech.PreNounAdjectival, EdictType.adj_pn },
            { PartOfSpeech.AuxiliaryVerb, EdictType.aux_v }
        };

        public IEnumerable<GlossNote> Gloss(string inputText)
        {
            var words = lang.BreakIntoSentences(inputText)
                .SelectMany(x => x)
                .ToList();

            var glosses = new List<GlossNote>();

            for(int i = 0; i < words.Count; i++)
            {
                var word = words[i];
                var greedySelection = words.Skip(i).Select(w => w.RawWord).Greedy(s =>
                {
                    var w = string.Join("", s);
                    return dict.Lookup(w) != null;
                }).ToList();
                var lookup = dict.Lookup(word.DictionaryForm ?? word.RawWord)?.ToList();

                if (word.RawWord.All(c => ".!?？！⁉、".IndexOf(c) != -1))
                {
                    // skip punctuation
                    continue;
                }
                if (word.Type == Option.Some(EdictType.vs_s))
                {
                    glosses.Add(CreateGloss(word, "suru, {0}", lookup));
                }
                if (word.Type == Option.Some(EdictType.vs_i))
                {
                    glosses.Add(CreateGloss(word, "suru, {0}, verbalizing suffix", lookup));
                }
                else if (mapping.TryGetValue(word.EstimatedPartOfSpeech, out var edictType))
                {
                    var description = lookup
                        ?.SelectMany(entry => entry.Senses)
                        .FirstOrDefault(s => s.Type.HasValue && s.Type == Option.Some(edictType))
                        ?.Description;
                    if((description == null || word.Type == Option.Some(EdictType.cop_da)) && greedySelection.Count > 1)
                    {
                        var greedyWord = string.Join("", greedySelection);
                        var greedyEntries = dict.Lookup(greedyWord);

                        var splitGreedyWord = string.Join(" ", lang.BreakIntoSentences(greedyWord).SelectMany(x => x).Select(x => x.RawWord));
                        glosses.Add(CreateGloss(new WordInfo(splitGreedyWord), "{0}", greedyEntries));

                        i += greedySelection.Count - 1; // -1 because iteration will result in one extra increase
                        continue;
                    }
                    description = description ?? lookup?.SelectMany(entry => entry.Senses).FirstOrDefault()?.Description ?? "";
                    glosses.Add(new GlossNote(word.RawWord, description));
                }
                else if (word.EstimatedPartOfSpeech == PartOfSpeech.Particle)
                {
                    var description = lookup
                        ?.SelectMany(entry => entry.Senses)
                        .FirstOrDefault(s => s.Type == Option.Some(EdictType.prt))
                        ?.Description;
                    if (description != null)
                    {
                        glosses.Add(new GlossNote(word.RawWord, "Particle " + word.DictionaryForm + " - " + description));
                    }
                    else
                    {
                        glosses.Add(CreateGloss(word, "{0}", lookup));
                    }
                }
                else if (word.Independent == false)
                {
                    var l = glosses.Last();
                    glosses.RemoveAt(glosses.Count - 1);
                    glosses.Add(new GlossNote(
                        l.Foreign + " " + word.RawWord,
                        l.Text.EndsWith(" + inflections") ? l.Text : l.Text + " + inflections"));
                }
                else
                {
                    glosses.Add(CreateGloss(word, "{0}", lookup));
                }
            };
            return glosses;
        }

        private GlossNote CreateGloss(WordInfo foreign, string format, IEnumerable<JMDictEntry> notInflected)
        {
            string senseString = "";
            if(notInflected != null)
            {
                JMDictSense sense = notInflected.SelectMany(e => e.Senses).FirstOrDefault(e => e.Type.HasValue && e.Type == foreign.Type);
                sense = sense ?? notInflected.SelectMany(e => e.Senses).First();
                senseString = sense.Description;
            }

            return new GlossNote(
                foreign.RawWord,
                (string.Format(format, senseString) + (foreign.RawWord != foreign.DictionaryForm && foreign.DictionaryForm != null ? " + inflections" : "")).Trim());
        }

        public AutoGlosser(ILanguageService lang, JMDict dict)
        {
            this.lang = lang;
            this.dict = dict;
        }
    }
}
