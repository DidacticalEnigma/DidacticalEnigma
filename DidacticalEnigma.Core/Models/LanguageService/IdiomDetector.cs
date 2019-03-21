﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JDict;
using Optional;
using Optional.Collections;
using TinyIndex;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class IdiomDetector : IDisposable
    {
        private static readonly Guid Version = new Guid("F77023E1-ED66-4844-B2E2-2A0203DEA665");

        private JMDict jmDict;

        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;

        private Database db;

        private IReadOnlyDiskArray<KeyValuePair<string, long>> entries;

        public class Result
        {
            public JMDictEntry DictionaryEntry { get; }

            public IEnumerable<(string fragment, bool highlight)> RenderedHighlights { get; }

            // arbitrary measure by which a specific result is "better" than other
            public double Similarity { get; }

            public Result(JMDictEntry dictionaryEntry, IEnumerable<(string fragment, bool highlight)> renderedHighlights, double similarity)
            {
                DictionaryEntry = dictionaryEntry;
                RenderedHighlights = renderedHighlights;
                Similarity = similarity;
            }
        }

        private List<(string fragment, bool highlight)> Concatenate(List<(string fragment, bool highlight)> input)
        {
            return input
                .GroupConsecutive(x => x.highlight)
                .Select(x => (string.Join("", x.Select(y => y.fragment)), x.First().highlight))
                .ToList();
        }

        private string OriginalNotNormalized(string normalized, JMDictEntry entry)
        {
            return entry.Kanji
                .Concat(entry.Readings)
                .FirstOrNone(s => Normalize(s) == normalized)
                .ValueOr(normalized);
        }

        private Option<Result> Rate(
            IReadOnlyList<IpadicEntry> queryMorphemes,
            string candidate,
            long candidateId)
        {
            return jmDict.LookupBySequenceNumber(candidateId).Map(candidateEntry =>
            {
                double similarity = 0.0;
                var remainder = candidate.Substring(candidate.IndexOf('\0')+1);
                var highlights = new List<(string fragment, bool highlight)>();
                
                candidate = candidate.Substring(0, candidate.IndexOf('\0'));
                var original = OriginalNotNormalized(remainder + candidate, candidateEntry);

                var analyzedCandidate = Analyze(original);
                for (int i = 0, j = 0; j < analyzedCandidate.Count;)
                {
                    // use a sentinel
                    var queryMorpheme = i < queryMorphemes.Count
                        ? queryMorphemes[i]
                        : new IpadicEntry("", Option.None<string>());
                    var candidateMorpheme = analyzedCandidate[j];

                    if (queryMorpheme.SurfaceForm == candidateMorpheme.SurfaceForm)
                    {
                        similarity++;
                        i++;
                        j++;
                        highlights.Add((candidateMorpheme.SurfaceForm, true));
                    }
                    else if (queryMorpheme.DictionaryForm != null && queryMorpheme.DictionaryForm == candidateMorpheme.DictionaryForm)
                    {
                        similarity += 0.5;
                        i++;
                        j++;
                        highlights.Add((candidateMorpheme.SurfaceForm, true));
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
                        i++;
                        j++;
                        highlights.Add((candidateMorpheme.SurfaceForm, false));
                    }
                }

                return new Result(candidateEntry, Concatenate(highlights), similarity / queryMorphemes.Count);
            });
        }

        public IEnumerable<Result> Detect(string p)
        {
            int limit = 50;
            var analyzed = Analyze(p);
            var normalized = Normalize(analyzed);
            var (start, end) = entries.EqualRange(normalized, kvp => kvp.Key);
            var outOfBoundLimit = (limit - (end - start))/2;
            start = Math.Max(0, start - outOfBoundLimit);
            end = Math.Min(entries.Count, end + outOfBoundLimit);
            var mid = (start + end)/2;
            return EnumerableExt.Range(start, end - start).Zip(entries.GetIdRange(start, end))
                .OrderBy(kvp => Math.Abs(kvp.Item1 - mid))
                .Select(kvp => Rate(analyzed, kvp.Item2.Key, kvp.Item2.Value))
                .Values()
                .Where(r => r.Similarity > 0)
                .OrderByDescending(r => r.Similarity)
                .DistinctBy(r => r.DictionaryEntry.SequenceNumber);
        }

        public IdiomDetector(
            JDict.JMDict jmDict,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            string cachePath)
        {
            this.jmDict = jmDict;
            this.analyzer = analyzer;
            this.db = Database.CreateOrOpen(cachePath, Version)
                .AddIndirectArray(Serializer.ForKeyValuePair(Serializer.ForStringAsUTF8(), Serializer.ForLong()),
                    CreateEntries, kvp => kvp.Key)
                .Build();

            this.entries = db.Get<KeyValuePair<string, long>>(0);
        }

        private IEnumerable<KeyValuePair<string, long>> CreateEntries()
        {
            var exprs = jmDict.AllEntries()
                .Where(entry =>
                    entry.Senses.Any(s =>
                        s.PartOfSpeechInfo.Any(pos => pos == EdictPartOfSpeech.exp)))
                .SelectMany(entry => entry.Kanji.Concat(entry.Readings).Select(e => new KeyValuePair<string, long>(Normalize(e), entry.SequenceNumber)));
            var rotations = exprs
                .SelectMany(e =>
                    StringExt.AllRotationsOf(e.Key + "\0")
                    .Select(r => new KeyValuePair<string, long>(r, e.Value))
                .Where(kvp => !kvp.Key.StartsWith("\0")));
            return rotations;
        }

        private string Normalize(string s)
        {
            var morphemes = Analyze(s);
            return Normalize(morphemes);
        }

        private string Normalize(IEnumerable<IpadicEntry> morphemes)
        {
            return string.Join("", morphemes
                .Where(e => e.PartOfSpeech != PartOfSpeech.Particle)
                .Select(m => m.DictionaryForm));
        }

        private IReadOnlyList<IpadicEntry> Analyze(string s)
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