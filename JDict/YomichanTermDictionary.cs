using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JDict.Json;
using LiteDB;
using Newtonsoft.Json;
using TinyIndex;
using Utility.Utils;
using FileMode = System.IO.FileMode;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace JDict
{
    // provides the programmatic access to dictionary files produced by
    // https://github.com/FooSoft/yomichan-import
    // 
    public class YomichanTermDictionary : IDisposable
    {
        private static readonly Guid Version = new Guid("F7D29471-7532-4CFC-AD18-75F26BFE406F");

        private TinyIndex.Database db;

        private IReadOnlyDiskArray<YomichanDictionaryEntry> entries;

        private IReadOnlyDiskArray<KeyValuePair<string, IReadOnlyList<long>>> index;

        private static readonly Regex termMatcher = new Regex(@"^term_bank_\d+.json$");

        public YomichanTermDictionary(string pathToZip, string cachePath)
        {
            using (var zip = new ZipFile(pathToZip))
            {
                Init(zip, cachePath);
            }
        }

        private void Init(IZipFile zip, string cachePath)
        {
            var entrySerializer = Serializer.ForComposite()
                .With(Serializer.ForStringAsUTF8())
                .With(Serializer.ForStringAsUTF8())
                .With(Serializer.ForStringAsUTF8())
                .With(Serializer.ForStringAsUTF8())
                .With(Serializer.ForInt())
                .With(Serializer.ForReadOnlyList(Serializer.ForStringAsUTF8()))
                .With(Serializer.ForInt())
                .With(Serializer.ForStringAsUTF8())
                .Create()
                .Mapping(
                    raw => new YomichanDictionaryEntry()
                    {
                        Expression = (string)raw[0],
                        Reading = (string)raw[1],
                        DefinitionTags = (string)raw[2],
                        Rules = (string)raw[3],
                        Score = (int)raw[4],
                        Glossary = (IReadOnlyList<string>)raw[5],
                        Sequence = (int)raw[6],
                        TermTags = (string)raw[7]
                    },
                    obj => new object[]
                    {
                        obj.Expression,
                        obj.Reading,
                        obj.DefinitionTags,
                        obj.Rules,
                        obj.Score,
                        obj.Glossary,
                        obj.Sequence,
                        obj.TermTags
                    });

            var indexSerializer = Serializer.ForKeyValuePair(
                Serializer.ForStringAsUTF8(),
                Serializer.ForReadOnlyList(Serializer.ForLong()));

            var lazyRoot = new Lazy<List<YomichanDictionaryEntry>>(() => ParseEntriesFromZip(zip).ToList());

            db = TinyIndex.Database.CreateOrOpen(cachePath, Version)
                .AddIndirectArray(entrySerializer, () => lazyRoot.Value)
                .AddIndirectArray(indexSerializer, () => Index(lazyRoot.Value), kvp => kvp.Key)
                .Build();

            entries = db.Get<YomichanDictionaryEntry>(0);
            index = db.Get<KeyValuePair<string, IReadOnlyList<long>>>(1);
        }

        private static IEnumerable<KeyValuePair<string, IReadOnlyList<long>>> Index(
            IReadOnlyList<YomichanDictionaryEntry> entries)
        {
            IEnumerable<KeyValuePair<long, string>> It()
            {
                foreach (var (e, i) in entries.Indexed())
                {
                    yield return new KeyValuePair<long, string>(i, e.Expression);
                    yield return new KeyValuePair<long, string>(i, e.Reading);
                }
            }

            return It()
                .GroupBy(kvp => kvp.Value, kvp => kvp.Key)
                .Select(x => new KeyValuePair<string, IReadOnlyList<long>>(x.Key, x.ToList()));
        }

        private IEnumerable<YomichanDictionaryEntry> ParseEntriesFromZip(IZipFile zip)
        {
            var groups = zip.Files.ToLookup(f =>
            {
                if (f == "index.json")
                    return 0;
                else if (termMatcher.IsMatch(f))
                    return 1;
                else
                    return 2;
            });
            var indexPath = groups[0].SingleOrDefault() ?? throw new InvalidDataException("not a valid yomichan dictionary");
            var dataFilePaths = groups[1].ToList();
            YomichanDictionaryVersion version;
            var serializer = new JsonSerializer();
            using (var stream = zip.OpenFile(indexPath))
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                version = serializer.Deserialize<YomichanDictionaryVersion>(jsonReader);
            }

            if (version.Format != 3)
            {
                throw new InvalidDataException("unsupported format");
            }

            foreach (var filePath in dataFilePaths)
            {
                using (var dataFile = zip.OpenFile(filePath))
                using (var reader = new StreamReader(dataFile))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    var entries = serializer.Deserialize<IEnumerable<YomichanDictionaryEntry>>(jsonReader);
                    foreach (var entry in entries)
                    {
                        yield return entry;
                    }
                }
            }
        }

        public YomichanTermDictionary(IZipFile zipFile, string cachePath)
        {
            Init(zipFile, cachePath);
        }

        public IEnumerable<Entry> Lookup(string key)
        {
            var result = index.BinarySearch(key, kvp => kvp.Key);
            if (result.id == -1)
                return null;

            return result.element.Value
                .Select(id => entries[id])
                .OrderBy(entry =>
                {
                    if (entry.Expression == key)
                        return 0;
                    if (entry.Reading == key)
                        return 1;
                    return 2;
                })
                .Select(entry => new Entry(entry.Expression, entry.Reading, entry.Glossary))
                .ToList();
        }

        private TOut With<TResource, TOut>(Func<TResource> resourceFactory, Func<TResource, TOut> resultFactory)
            where TResource : IDisposable
        {
            using (var resource = resourceFactory())
            {
                return resultFactory(resource);
            }
        }

        public void Dispose()
        {
            db.Dispose();
        }

        public class Entry
        {
            public Entry(string expression, string reading, IEnumerable<string> glossary)
            {
                Expression = expression ?? throw new ArgumentNullException(nameof(expression));
                Reading = reading;
                Glossary = glossary?.ToList() ?? throw new ArgumentNullException(nameof(glossary));
            }

            public string Expression { get; }

            public string Reading { get; }

            public IEnumerable<string> Glossary { get; }

            public override string ToString()
            {
                return Expression + "\n" + Reading + "\n" + string.Join("\n", Glossary) + "\n";
            }
        }
    }
}
