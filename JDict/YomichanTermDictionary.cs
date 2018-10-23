using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JDict.Json;
using JDict.Utils;
using LiteDB;
using Newtonsoft.Json;
using FileMode = System.IO.FileMode;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace JDict
{
    // provides the programmatic access to dictionary files produced by
    // https://github.com/FooSoft/yomichan-import
    // 
    public class YomichanTermDictionary : IDisposable
    {
        private static readonly int Version = 3;

        private static readonly BsonMapper mapper = new BsonMapper
        {
            TrimWhitespace = false,
            EmptyStringToNull = false,
            // I'm fine with this - I don't use typeless schemas
            SerializeNullValues = false
        };

        private LiteDatabase db;

        private LiteCollection<YomichanDictionaryEntry> entries;

        private LiteCollection<YomichanDictionaryInfo> version;

        private static readonly Regex termMatcher = new Regex(@"^term_bank_\d+.json$");

        public string Revision { get; private set; }

        public YomichanTermDictionary(string pathToZip, string cache)
        {
            using (var zip = new ZipFile(pathToZip))
            {
                Init(zip, File.Open(cache, FileMode.OpenOrCreate));
            }
        }

        private void Init(IZipFile zip, Stream cacheFile)
        {
            db = new LiteDatabase(cacheFile, mapper);
            entries = db.GetCollection<YomichanDictionaryEntry>("entries");
            this.version = db.GetCollection<YomichanDictionaryInfo>("version");
            var versionInfo = version.FindAll().SingleOrDefault();
            if (versionInfo == null ||
                versionInfo.DbVersion != Version)
            {
                FillDatabase(zip);
            }
            else
            {
                Revision = versionInfo.Title+"/"+versionInfo.Revision;
            }
        }

        private void FillDatabase(IZipFile zip)
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

            this.version.Delete(_ => true);
            this.entries.Delete(_ => true);
            this.entries.EnsureIndex(e => e.Expression);
            this.entries.EnsureIndex(e => e.Reading);

            foreach (var filePath in dataFilePaths)
            {
                using (var dataFile = zip.OpenFile(filePath))
                using (var reader = new StreamReader(dataFile))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    var entries = serializer.Deserialize<IEnumerable<YomichanDictionaryEntry>>(jsonReader);
                    this.entries.InsertBulk(entries);
                }
            }

            this.version.Insert(new YomichanDictionaryInfo()
            {
                DbVersion = Version,
                OriginalFileSize = -1,
                Revision = version.Revision,
                Title = version.Title
            });
        }

        public YomichanTermDictionary(IZipFile zipFile, string cache)
        {
            Init(zipFile, File.Open(cache, FileMode.OpenOrCreate));
        }

        public IEnumerable<Entry> Lookup(string key)
        {
            var l = entries.Find(entry => entry.Expression == key || entry.Reading == key)
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
            if (l.Count == 0)
                return null;
            return l;
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
