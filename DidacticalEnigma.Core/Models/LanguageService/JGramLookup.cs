using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JDict;
using Optional;
using Optional.Collections;
using TinyIndex;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IJGramLookup : IDisposable
    {
        IEnumerable<JGram.Entry> Lookup(string key);
    }

    public class JGramLookup : IJGramLookup
    {
        private static readonly Guid Version = new Guid("2A7D5BC6-9ECA-47FE-B00A-6A08D225CA81");

        private readonly IReadOnlyDiskArray<JGram.Entry> entries;

        private readonly IReadOnlyDiskArray<KeyValuePair<string, long>> index;

        private readonly Database db;

        private IEnumerable<KeyValuePair<string, long>> LoadIndexEntries(string path)
        {
            foreach (var line in File.ReadLines(path, Encoding.UTF8))
            {
                if (line.StartsWith("？", StringComparison.Ordinal) || line.StartsWith("#", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(line))
                    continue;

                var components = line.Split('\t');
                var id = long.Parse(components[0]);
                components = components[1].Split('・');
                foreach (var lookup in components.Select(c => c.Trim()).Distinct())
                {
                    // TODO: handle split entries
                    if (lookup.Contains("～"))
                        continue;

                    yield return new KeyValuePair<string, long>(lookup, id);
                }
            }
        }

        private int CommonPrefixLength(string left, string right)
        {
            return left.AsCodePoints()
                .Zip(right.AsCodePoints(), (l, r) => l == r)
                .TakeWhile(p => p)
                .Count();
        }

        public IEnumerable<JGram.Entry> Lookup(string key)
        {
            int limit = 50;
            var (start, end) = index.EqualRange(key, kvp => kvp.Key);
            var outOfBoundLimit = (limit - (end - start)) / 2;
            start = Math.Max(0, start - outOfBoundLimit);
            end = Math.Min(entries.Count, end + outOfBoundLimit);
            var mid = (start + end) / 2;
            return EnumerableExt.Range(start, end - start)
                .OrderBy(i => Math.Abs(i - mid))
                .Select(i => index[i])
                .Select(indexEntry =>
                {
                    var indexKey = indexEntry.Key;
                    var entryKey = indexEntry.Value;
                    var (entry, id) = entries.BinarySearch(entryKey, e => e.Id);
                    return (indexKey, entry).SomeWhen(_ => id != -1);
                })
                .Values()
                .OrderByDescending(r => CommonPrefixLength(r.indexKey, key))
                .Where(r => CommonPrefixLength(r.indexKey, key) != 0)
                .Select(r => r.entry)
                .DistinctBy(entry => entry.Id);
        }

        public JGramLookup(string jgramPath, string jgramLookupPath, string cachePath)
        {
            var entrySerializer = Serializer.ForComposite()
                .With(Serializer.ForLong())
                .With(Serializer.ForStringAsUTF8())
                .With(Serializer.ForStringAsUTF8())
                .With(Serializer.ForStringAsUTF8())
                .With(Serializer.ForStringAsUTF8())
                .With(Serializer.ForStringAsUTF8())
                .Create()
                .Mapping(raw => new JGram.Entry(
                        (long)raw[0],
                        EmptyToNull((string)raw[1]),
                        EmptyToNull((string)raw[2]),
                        EmptyToNull((string)raw[3]),
                        EmptyToNull((string)raw[4]),
                        EmptyToNull((string)raw[5])),
                    obj => new object[]
                    {
                        obj.Id,
                        NullToEmpty(obj.Key),
                        NullToEmpty(obj.Reading),
                        NullToEmpty(obj.Romaji),
                        NullToEmpty(obj.Translation),
                        NullToEmpty(obj.Example)
                    });

            var indexSerializer = Serializer.ForKeyValuePair(
                Serializer.ForStringAsUTF8(),
                Serializer.ForLong());

            db = Database.CreateOrOpen(cachePath, Version)
                .AddIndirectArray(entrySerializer, db => JGram.Parse(jgramPath, Encoding.UTF8), e => e.Id)
                .AddIndirectArray(indexSerializer, db => LoadIndexEntries(jgramLookupPath), kvp => kvp.Key)
                .Build();

            entries = db.Get<JGram.Entry>(0);
            index = db.Get<KeyValuePair<string, long>>(1);

            string NullToEmpty(string s)
            {
                return s ?? "";
            }

            string EmptyToNull(string s)
            {
                return s == "" ? null : s;
            }
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
