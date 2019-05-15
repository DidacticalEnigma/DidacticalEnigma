using System;
using System.Collections.Generic;
using System.Linq;
using JDict;
using Optional;
using Optional.Collections;
using TinyIndex;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class Corpus : IDisposable
    {
        private static readonly Guid Version = new Guid("1E350DD3-1F74-4F23-8C95-46BA3BB4BD1E");

        private readonly IMorphologicalAnalyzer<IEntry> analyzer;

        private Database db;

        private IReadOnlyDiskArray<SentencePair> entries;

        private IReadOnlyDiskArray<KeyValuePair<string, long>> index;

        public class Result
        {
            public SentencePair SentencePair { get; }

            public IEnumerable<(string fragment, bool highlight)> RenderedHighlights { get; }

            // arbitrary measure by which a specific result is "better" than other
            public double Similarity { get; }

            public Result(SentencePair sentencePair, IEnumerable<(string fragment, bool highlight)> renderedHighlights, double similarity)
            {
                SentencePair = sentencePair;
                RenderedHighlights = renderedHighlights;
                Similarity = similarity;
            }
        }

        private List<(string fragment, bool highlight)> Concatenate(List<(string fragment, bool highlight)> input)
        {
            return input
                .GroupConsecutive(x => x.highlight)
                .Select(x => x.Materialize())
                .Select(x => (string.Join("", x.Select(y => y.fragment)), x.First().highlight))
                .ToList();
        }

        private string OriginalNotNormalized(string normalized, SentencePair sentencePair)
        {
            return sentencePair.JapaneseSentence;
        }

        private double Similarity(IEntry left, IEntry right)
        {
            if (left.SurfaceForm == right.SurfaceForm)
            {
                return 1.0;
            }
            else if (left.DictionaryForm != null && left.DictionaryForm == right.DictionaryForm)
            {
                return 0.5;
            }

            return 0.0;
        }

        private Result Rate(
            IReadOnlyList<IEntry> queryMorphemes,
            string candidate,
            long candidateId)
        {
            var candidateEntry = entries[candidateId];
            double similarity = 0.0;
            var remainder = candidate.Substring(candidate.IndexOf('\0') + 1);
            var highlights = new List<(string fragment, bool highlight)>();

            candidate = candidate.Substring(0, candidate.IndexOf('\0'));
            var original = OriginalNotNormalized(remainder + candidate, candidateEntry);

            int j = 0;
            var analyzedCandidate = Analyze(original);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            while (j < analyzedCandidate.Count && Similarity(queryMorphemes[0], analyzedCandidate[j]) == 0.0)
            {
                highlights.Add((analyzedCandidate[j].SurfaceForm, false));
                j++;
            }

            bool mismatch = false;
            for (int i = 0; j < analyzedCandidate.Count;)
            {
                // use a sentinel
                var queryMorpheme = i < queryMorphemes.Count
                    ? queryMorphemes[i]
                    : new IpadicEntry("", Option.None<string>());
                var candidateMorpheme = analyzedCandidate[j];

                if (mismatch)
                {
                    i++;
                    j++;
                    highlights.Add((candidateMorpheme.SurfaceForm, false));
                    continue;
                }

                var morphemeSimilarity = Similarity(queryMorpheme, candidateMorpheme);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (morphemeSimilarity != 0.0)
                {
                    similarity += morphemeSimilarity;
                    i++;
                    j++;
                    highlights.Add((candidateMorpheme.SurfaceForm, true));
                }
                else if (queryMorpheme.PartOfSpeech == PartOfSpeech.Particle && candidateMorpheme.PartOfSpeech == PartOfSpeech.Particle)
                {
                    i++;
                    j++;
                    highlights.Add((candidateMorpheme.SurfaceForm, false));
                }
                else if (queryMorpheme.PartOfSpeech == PartOfSpeech.Particle)
                {
                    i++;
                }
                else if (candidateMorpheme.PartOfSpeech == PartOfSpeech.Particle)
                {
                    j++;
                    highlights.Add((candidateMorpheme.SurfaceForm, false));
                }
                else
                {
                    mismatch = true;
                    i++;
                    j++;
                    highlights.Add((candidateMorpheme.SurfaceForm, false));
                }
            }

            return new Result(candidateEntry, Concatenate(highlights), similarity / queryMorphemes.Count);
        }

        public IEnumerable<Result> GetSentences(string p, int limit = 50)
        {
            var analyzed = Analyze(p);
            var normalized = Normalize(analyzed);
            var (start, end) = index.EqualRange(normalized, kvp => kvp.Key);
            var outOfBoundLimit = (limit - (end - start)) / 2;
            start = Math.Max(0, start - outOfBoundLimit);
            end = Math.Min(index.Count, end + outOfBoundLimit);
            var mid = (start + end) / 2;
            return EnumerableExt.Range(start, end - start).Zip(index.GetIdRange(start, end))
                .DistinctBy(kvp => kvp.Item2.Value)
                .OrderBy(kvp => Math.Abs(kvp.Item1 - mid))
                .Select(kvp => Rate(analyzed, kvp.Item2.Key, kvp.Item2.Value))
                .Where(r => r.Similarity > 0)
                .OrderByDescending(r => r.Similarity);
        }

        // there is no reason why Corpus requires IPADIC
        // outside of normalization consistency between program runs
        public Corpus(
            Func<IEnumerable<SentencePair>> sentenceFactory,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            string cachePath)
        {
            this.analyzer = analyzer;
            var sentenceSerializer = Serializer.ForComposite()
                .With(Serializer.ForStringAsUTF8())
                .With(Serializer.ForStringAsUTF8())
                .Create()
                .Mapping(
                    raw => new SentencePair((string) raw[0], (string) raw[1]),
                    obj => new object[] {obj.JapaneseSentence, obj.EnglishSentence});

            db = Database.CreateOrOpen(cachePath, Version)
                .AddIndirectArray(sentenceSerializer, db => sentenceFactory())
                .AddIndirectArray(Serializer.ForKeyValuePair(Serializer.ForStringAsUTF8(), Serializer.ForLong()),
                    db => CreateIndex(db.Get<SentencePair>(0).LinearScan()), kvp => kvp.Key)
                .Build();

            entries = db.Get<SentencePair>(0);
            index = db.Get<KeyValuePair<string, long>>(1);
        }

        private IEnumerable<KeyValuePair<string, long>> CreateIndex(IEnumerable<SentencePair> sentences)
        {
            var rotations = sentences
                .Indexed()
                .SelectMany(e =>
                    StringExt.AllRotationsOf(e.element.JapaneseSentence + "\0")
                        .Select(r => new KeyValuePair<string, long>(r, e.index))
                        .Where(kvp => !kvp.Key.StartsWith("\0", StringComparison.Ordinal)));
            return rotations;
        }

        private string Normalize(string s)
        {
            var morphemes = Analyze(s);
            return Normalize(morphemes);
        }

        private string Normalize(IEnumerable<IEntry> morphemes)
        {
            return string.Join("", morphemes
                .Where(e => e.PartOfSpeech != PartOfSpeech.Particle)
                .Select(m => m.DictionaryForm));
        }

        private IReadOnlyList<IEntry> Analyze(string s)
        {
            var morphemes = analyzer.ParseToEntries(s)
                .Where(e => e.IsRegular)
                .ToList();
            return morphemes;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}