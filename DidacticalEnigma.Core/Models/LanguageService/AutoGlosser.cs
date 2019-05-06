using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DidacticalEnigma.Core.Models.Project;
using DidacticalEnigma.Core.Utils;
using JDict;
using JetBrains.Annotations;
using Optional;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class AutoGlosserNote : IEquatable<AutoGlosserNote>
    {
        [NotNull] public string Foreign { get; }

        [NotNull] public IReadOnlyCollection<string> GlossCandidates { get; }

        public AutoGlosserNote([NotNull] string foreign, [NotNull] IEnumerable<string> glossCandidates)
        {
            Foreign = foreign ?? throw new ArgumentNullException(nameof(foreign));
            var glosses = glossCandidates ?? throw new ArgumentNullException(nameof(glossCandidates));
            GlossCandidates = glosses.ToList();
        }

        public bool Equals(AutoGlosserNote other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Foreign, other.Foreign) && GlossCandidates.SequenceEqual(other.GlossCandidates);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AutoGlosserNote)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Foreign.GetHashCode() * 397) ^ GlossCandidates.DefaultIfEmpty("").Aggregate(0, (h, s) => h * 397 + s.GetHashCode());
            }
        }

        public static bool operator ==(AutoGlosserNote left, AutoGlosserNote right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AutoGlosserNote left, AutoGlosserNote right)
        {
            return !Equals(left, right);
        }
    }

    public interface IAutoGlosser
    {
        IEnumerable<AutoGlosserNote> Gloss(string inputText);
    }

    public class AutoGlosserNext : IAutoGlosser
    {
        private readonly ISentenceParser parser;
        private readonly JMDictLookup dictLookup;
        private readonly IKanaProperties kana;

        public AutoGlosserNext(ISentenceParser parser, JMDictLookup dictLookup, IKanaProperties kana)
        {
            this.parser = parser;
            this.dictLookup = dictLookup;
            this.kana = kana;
        }

        public IEnumerable<AutoGlosserNote> Gloss(string inputText)
        {
            var words = parser.BreakIntoSentences(inputText)
                .SelectMany(x => x)
                .ToList();

            var glosses = new List<AutoGlosserNote>();

            for (int i = 0; i < words.Count; i++)
            {
                var word = words[i];
                var greedySelection = words.Skip(i).Greedy(wordInfos =>
                {
                    var entireExpression = string.Concat(wordInfos.Select(w => w.RawWord));
                    var l = dictLookup.Lookup(entireExpression);
                    if (l == null)
                        return false;

                    var entireReading = kana.ToHiragana(string.Concat(wordInfos.Select(w => w.Reading)));
                    if (l.Any(e => e.Readings.Any(r => entireReading == kana.ToHiragana(r))))
                    {
                        return true;
                    }

                    return false;
                }).ToList();
                var lookup = dictLookup.Lookup(word.DictionaryForm ?? word.RawWord)?.ToList();

                if (word.RawWord.All(c => ".!?？！⁉、".IndexOf(c) != -1))
                {
                    // skip punctuation
                    continue;
                }

                if (greedySelection.Count > 1)
                {
                    var greedyLookup = dictLookup.Lookup(string.Concat(greedySelection.Select(w => w.RawWord))).Materialize();
                    glosses.Add(new AutoGlosserNote(
                        string.Join(" ", greedySelection.Select(w => w.RawWord)),
                        OrderSenses(FilterOutInapplicableSenses(greedyLookup, greedySelection)).Select(FormatSense)));

                    i += greedySelection.Count - 1; // -1 because iteration will result in one extra increase
                    continue;
                }
                else if (lookup != null)
                {
                    glosses.Add(new AutoGlosserNote(word.RawWord, OrderSenses(FilterOutInapplicableSenses(lookup, word)).Select(FormatSense)));
                }
                else
                {
                    glosses.Add(new AutoGlosserNote(word.RawWord, new string[0]));
                }
            }

            return glosses;
        }

        private string FormatSense(JMDictSense s)
        {
            var sb = new StringBuilder();
            if (s.Informational.Any())
            {
                sb.Append("(");
                bool first = true;
                foreach (var info in s.Informational)
                {
                    if (!first)
                        sb.Append("/");
                    sb.Append(info);
                    first = false;
                }

                sb.Append(") ");
            }

            {
                bool first = true;
                foreach (var gloss in s.Glosses)
                {
                    if (!first)
                        sb.Append("/");
                    sb.Append(gloss);
                    first = false;
                }
            }
            return sb.ToString();
        }

        private IEnumerable<JMDictSense> OrderSenses(IEnumerable<JMDictSense> senses)
        {
            return senses.OrderBy(s =>
            {
                if (s.PartOfSpeechInfo.Contains(EdictPartOfSpeech.exp))
                    return 0;
                return 1;
            });
        }

        private IEnumerable<JMDictSense> FilterOutInapplicableSenses(
            IEnumerable<JMDictEntry> entries,
            WordInfo wordInfo)
        {
            foreach (var entry in entries)
            {
                if (wordInfo.DictionaryForm == null || wordInfo.DictionaryForm == wordInfo.RawWord)
                {
                    var wordReading = kana.ToHiragana(wordInfo.Reading);
                    if (entry.Readings.All(r => wordReading != kana.ToHiragana(r)))
                    {
                        continue;
                    }
                }

                foreach (var sense in entry.Senses)
                {
                    if (wordInfo.EstimatedPartOfSpeech == PartOfSpeech.Particle &&
                        !sense.PartOfSpeechInfo.Contains(EdictPartOfSpeech.prt))
                        continue;

                    yield return sense;
                }
            }
        }

        private IEnumerable<JMDictSense> FilterOutInapplicableSenses(
            IEnumerable<JMDictEntry> entries,
            IReadOnlyCollection<WordInfo> wordInfos)
        {
            foreach (var entry in entries)
            {
                foreach (var sense in entry.Senses)
                {
                    yield return sense;
                }
            }
        }
    }
}
