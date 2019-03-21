using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.Project;
using JDict;
using Optional;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class AutoGlosser
    {
        private readonly IMorphologicalAnalyzer<IEntry> morphologicalAnalyzer;
        private readonly JMDict dict;

        private static readonly IReadOnlyDictionary<PartOfSpeech, EdictPartOfSpeech> mapping = new Dictionary<PartOfSpeech, EdictPartOfSpeech>
        {
            { PartOfSpeech.PreNounAdjectival, EdictPartOfSpeech.adj_pn },
            { PartOfSpeech.AuxiliaryVerb, EdictPartOfSpeech.aux_v }
        };

        public IEnumerable<GlossNote> Gloss(string inputText)
        {
            var words = morphologicalAnalyzer.BreakIntoSentences(inputText)
                .SelectMany(x => x)
                .ToList();

            var glosses = new List<GlossNote>();

            for(int i = 0; i < words.Count; i++)
            {
                var word = words[i];
                var greedySelection = words.Skip(i).Select(w => w.RawWord).Greedy(s =>
                {
                    var w = String.Join("", s);
                    return dict.Lookup(w) != null;
                }).ToList();
                var lookup = dict.Lookup(word.DictionaryForm ?? word.RawWord)?.ToList();

                if (word.RawWord.All(c => ".!?？！⁉、".IndexOf(c) != -1))
                {
                    // skip punctuation
                    continue;
                }
                if (word.Type == Option.Some(EdictPartOfSpeech.vs_s))
                {
                    glosses.Add(CreateGloss(word, "suru, {0}", lookup));
                }
                if (word.Type == Option.Some(EdictPartOfSpeech.vs_i))
                {
                    glosses.Add(CreateGloss(word, "suru, {0}, verbalizing suffix", lookup));
                }
                else if (mapping.TryGetValue(word.EstimatedPartOfSpeech, out var edictType))
                {
                    var description = CreateDescription((lookup
                        ?.SelectMany(entry => entry.Senses)
                        .FirstOrDefault(s => s.Type.HasValue && s.Type == Option.Some(edictType))));
                    if((description == null || word.Type == Option.Some(EdictPartOfSpeech.cop_da)) && greedySelection.Count > 1)
                    {
                        var greedyWord = String.Join("", greedySelection);
                        var greedyEntries = dict.Lookup(greedyWord);

                        var splitGreedyWord = String.Join(" ", morphologicalAnalyzer.BreakIntoSentences(greedyWord).SelectMany(x => x).Select(x => x.RawWord));
                        glosses.Add(CreateGloss(new WordInfo(splitGreedyWord), "{0}", greedyEntries));

                        i += greedySelection.Count - 1; // -1 because iteration will result in one extra increase
                        continue;
                    }
                    description = description ?? CreateDescription((lookup?.SelectMany(entry => entry.Senses).FirstOrDefault())) ?? "";
                    glosses.Add(new GlossNote(word.RawWord, description));
                }
                else if (word.EstimatedPartOfSpeech == PartOfSpeech.Particle)
                {
                    var description = CreateDescription((lookup
                        ?.SelectMany(entry => entry.Senses)
                        .FirstOrDefault(s => s.Type == Option.Some(EdictPartOfSpeech.prt))));
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
                senseString = CreateDescription(sense);
            }

            return new GlossNote(
                foreign.RawWord,
                (string.Format(format, senseString) + (foreign.RawWord != foreign.DictionaryForm && foreign.DictionaryForm != null ? " + inflections" : "")).Trim());
        }

        public AutoGlosser(IMorphologicalAnalyzer<IEntry> morphologicalAnalyzer, JMDict dict)
        {
            this.morphologicalAnalyzer = morphologicalAnalyzer;
            this.dict = dict;
        }

        private static string CreateDescription(JMDictSense sense)
        {
            return string.Join("/", sense.Glosses);
        }
    }
}
