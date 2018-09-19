using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DidacticalEnigma.Core.Models.Project;
using JDict;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class AutoGlosser
    {
        private readonly ILanguageService lang;
        private readonly JMDict dict;

        public IEnumerable<GlossNote> Gloss(string inputText)
        {
            var words = lang.BreakIntoSentences(inputText)
                .SelectMany(x => x)
                .ToList();

            var glosses = new List<GlossNote>();

            foreach(var word in words)
            {
                if (word.EstimatedPartOfSpeech == PartOfSpeech.Particle)
                {
                    var lookup = dict.Lookup(word.NotInflected);
                    var description = lookup
                        ?.SelectMany(entry => entry.Senses)
                        .First(s => s.Type == EdictType.prt)
                        .Description;
                    glosses.Add(new GlossNote(word.RawWord, "Particle " + word.NotInflected + " - " + description));
                }
                else if (word.Type == EdictType.vs_i)
                {
                    glosses.Add(new GlossNote(
                        word.RawWord,
                        ("suru, " + (dict.Lookup(word.NotInflected).FirstOrDefault()?.Senses.First().Description) + ", verbalizing suffix" +
                         (word.RawWord != word.NotInflected ? " + inflections" : "")).Trim()));
                }
                else if (word.Independent == false || word.EstimatedPartOfSpeech == PartOfSpeech.AuxiliaryVerb)
                {
                    var l = glosses.Last();
                    glosses.RemoveAt(glosses.Count - 1);
                    glosses.Add(new GlossNote(
                        l.Foreign + " " + word.RawWord,
                        l.Text.EndsWith(" + inflections") ? l.Text : l.Text + " + inflections"));
                }
                else
                {
                    glosses.Add(new GlossNote(
                        word.RawWord,
                        ((dict.Lookup(word.NotInflected).FirstOrDefault()?.Senses.First().Description) +
                         (word.RawWord != word.NotInflected ? " + inflections" : "")).Trim()));
                }
            };
            return glosses;
        }

        public AutoGlosser(ILanguageService lang, JMDict dict)
        {
            this.lang = lang;
            this.dict = dict;
        }
    }
}
