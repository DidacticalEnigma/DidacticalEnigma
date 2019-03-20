using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JDict;
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

        public IEnumerable<JMDictEntry> Detect(string p)
        {
            int limit = 50;
            var normalized = Normalize(p);
            var (start, end) = entries.EqualRange(normalized, kvp => kvp.Key);
            var outOfBoundLimit = (limit - (end - start))/2;
            start = Math.Max(0, start - outOfBoundLimit);
            end = Math.Min(entries.Count, end + outOfBoundLimit);
            var mid = (start + end)/2;
            return EnumerableExt.Range(start, end - start).Zip(entries.GetIdRange(start, end))
                .OrderBy(kvp => Math.Abs(kvp.Item1 - mid))
                .Select(kvp => jmDict.LookupBySequenceNumber(kvp.Item2.Value))
                .Values();
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
            var morphemes = analyzer.ParseToEntries(s)
                .Where(e => e.IsRegular)
                .Where(e => e.PartOfSpeech != PartOfSpeech.Particle)
                .Select(e => e.DictionaryForm);

            return string.Join("", morphemes);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
