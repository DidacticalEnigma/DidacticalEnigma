using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JDict;
using TinyIndex;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IJGramLookup : IDisposable
    {
        IEnumerable<JGram.Entry> Lookup(string key);
    }

    public class JGramLookup : IJGramLookup
    {
        private static readonly Guid Version = new Guid("17FF94CC-0EC9-4D98-A975-1D0321FE6ABE");

        private IReadOnlyDiskArray<JGram.Entry> entries;

        private IReadOnlyDiskArray<KeyValuePair<string, long>> index;

        private Database db;

        private IEnumerable<KeyValuePair<string, long>> CreateIndexEntriesFor(JGram.Entry entry)
        {
            yield break;
        }

        public IEnumerable<JGram.Entry> Lookup(string key)
        {
            yield break;
        }

        public JGramLookup(string jgramPath, string cachePath)
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
                        (long) raw[0],
                        (string) raw[1],
                        (string) raw[2],
                        (string) raw[3],
                        (string) raw[4],
                        (string) raw[5]),
                    obj => new object[]
                    {
                        obj.Id,
                        obj.Key,
                        obj.Reading,
                        obj.Romaji,
                        obj.Translation,
                        obj.Example
                    });

            var indexSerializer = Serializer.ForKeyValuePair(
                Serializer.ForStringAsUTF8(),
                Serializer.ForLong());

            var lazyRoot = new Lazy<IEnumerable<JGram.Entry>>(() =>
            {
                using (var reader = File.OpenText(jgramPath))
                {
                    return JGram.Parse(reader).ToList();
                }
            });

            db = Database.CreateOrOpen(cachePath, Version)
                .AddIndirectArray(entrySerializer, () => lazyRoot.Value)
                .AddIndirectArray(indexSerializer, () => lazyRoot.Value.SelectMany(CreateIndexEntriesFor))
                .Build();

            entries = db.Get<JGram.Entry>(0);
            index = db.Get<KeyValuePair<string, long>>(1);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
