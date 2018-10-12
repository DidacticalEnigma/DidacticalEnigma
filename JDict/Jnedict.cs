using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using JDict.Internal.XmlModels;
using JDict.Utils;
using LiteDB;
using Optional;
using Optional.Collections;
using FileMode = System.IO.FileMode;

namespace JDict
{
    public class Jnedict : IDisposable
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(JMNedictRoot));

        private static readonly int Version = 1;

        private LiteCollection<DbDictVersion> version;

        private LiteCollection<DbNeDictKeyValue> kvps;

        private LiteCollection<DbNeEntry> entries;

        private LiteCollection<DbNeTranslation> trans;

        private LiteDatabase db;

        public void Dispose()
        {
            db.Dispose();
        }

        private Jnedict Init(Stream stream, LiteDatabase cache)
        {
            db = cache;
            kvps = db.GetCollection<DbNeDictKeyValue>("kvps");
            entries = db.GetCollection<DbNeEntry>("entries");
            trans = db.GetCollection<DbNeTranslation>("trans");
            version = cache.GetCollection<DbDictVersion>("version");
            var versionInfo = version.FindAll().FirstOrDefault();
            if (versionInfo == null ||
                (versionInfo.OriginalFileSize != -1 && stream.CanSeek &&
                 stream.Length != versionInfo.OriginalFileSize) ||
                versionInfo.DbVersion != Version)
            {
                var root = ReadFromXml(stream);
                FillDatabase(root);
            }

            return this;
        }

        private void FillDatabase(JMNedictRoot root)
        {
            kvps.Delete(_ => true);
            entries.Delete(_ => true);
            trans.Delete(_ => true);
            version.Delete(_ => true);

            kvps.EnsureIndex(x => x.LookupKey);

            var transDict = root.Entries
                .SelectMany(e => e.TranslationalEquivalents
                    .Select(tr => (xmlModel: tr, dbModel: new DbNeTranslation()
                    {
                        Type = (tr.Types ?? Array.Empty<string>())
                            .Select(t => JnedictTypeUtils.FromDescription(t))
                            .Values()
                            .ToList(),
                        Detail = (tr.Translation ?? Array.Empty<NeTranslation>())
                            .Where(t => t.Lang == null || t.Lang == "eng")
                            .Select(t => t.Text)
                            .ToList()
                    })))
                .ToDictionary(kvp => kvp.xmlModel, kvp => kvp.dbModel);

            {
                int id = 1; // to not make it start from 0
                foreach (var t in transDict)
                {
                    t.Value.Id = id++;
                }
            }

            var entriesDict = root.Entries
                .Select((e, id) => (xmlModel: e, dbModel: new DbNeEntry
                {
                    Id = id + 1, // to not make it start from 0
                    SequenceNumber = e.SequenceNumber,
                    Kanji = (e.KanjiElements ?? Array.Empty<KanjiElement>())
                        .Select(k => k.Key)
                        .ToList(),
                    Reading = (e.ReadingElements ?? Array.Empty<ReadingElement>())
                        .Select(r => r.Reb)
                        .ToList(),
                    Translation = (e.TranslationalEquivalents ?? Array.Empty<NeTranslationalEquivalent>())
                        .Select(t => transDict[t]).ToList()
                }))
                .ToDictionary(kvp => kvp.xmlModel, kvp => kvp.dbModel);

            var kvpsDict = new Dictionary<string, List<DbNeEntry>>();
            var kvpsEn = root.Entries
                .SelectMany(e =>
                    (e.KanjiElements?.Select(k => k.Key) ?? Enumerable.Empty<string>())
                    .Concat(e.ReadingElements.Select(r => r.Reb))
                    .Select(k => (key: k, value: e)));
            foreach (var (key, value) in kvpsEn)
            {
                if (!kvpsDict.ContainsKey(key))
                    kvpsDict[key] = new List<DbNeEntry>();
                kvpsDict[key].Add(entriesDict[value]);
            }

            trans.InsertBulk(transDict.Values);
            entries.InsertBulk(entriesDict.Values);
            kvps.InsertBulk(kvpsDict.Select(kvp => new DbNeDictKeyValue
            {
                LookupKey = kvp.Key,
                Entries = kvp.Value
            }));

            version.Insert(new DbDictVersion
            {
                DbVersion = Version,
                OriginalFileSize = -1,
                OriginalFileHash = Array.Empty<byte>()
            });
        }

        public IEnumerable<JnedictEntry> Lookup(string key)
        {
            return kvps
                .IncludeAll()
                .FindOne(kvp => kvp.LookupKey == key)
                ?.Entries
                ?.Select(e => new JnedictEntry(
                    e.SequenceNumber,
                    e.Kanji,
                    e.Reading,
                    e.Translation.Select(t => new JnedictTranslation(t.Type, t.Detail))));
        }

        private JMNedictRoot ReadFromXml(Stream stream)
        {
            var xmlSettings = new XmlReaderSettings
            {
                CloseInput = false,
                DtdProcessing = DtdProcessing.Parse, // we have local entities
                XmlResolver = null, // we don't want to resolve against external entities
                MaxCharactersFromEntities = 128 * 1024 * 1024 / 2, // 128 MB
                MaxCharactersInDocument = 512 * 1024 * 1024 / 2 // 512 MB
            };
            using (var xmlReader = XmlReader.Create(stream, xmlSettings))
            {
                return ((JMNedictRoot)serializer.Deserialize(xmlReader));
            }
        }

        private async Task<Jnedict> InitAsync(Stream stream, LiteDatabase cache)
        {
            // TODO: not a lazy way
            await Task.Run(() => Init(stream, cache));
            return this;
        }

        private async Task<Jnedict> InitAsync(string path, LiteDatabase cache)
        {
            using (var file = File.OpenRead(path))
            using (var gzip = new GZipStream(file, CompressionMode.Decompress))
            {
                return await InitAsync(gzip, cache);
            }
        }

        private Jnedict Init(string path, LiteDatabase cache)
        {
            using (var file = File.OpenRead(path))
            using (var gzip = new GZipStream(file, CompressionMode.Decompress))
            {
                return Init(gzip, cache);
            }
        }

        private Jnedict()
        {

        }

        public static Jnedict Create(string path, string cache)
        {
            return new Jnedict().Init(
                path,
                OpenDatabase(File.Open(cache, FileMode.OpenOrCreate), dispose: true));
        }

        public static Jnedict Create(Stream stream, Stream cache)
        {
            return new Jnedict().Init(
                stream,
                OpenDatabase(cache, dispose: false));
        }

        public static async Task<Jnedict> CreateAsync(string path, string cache)
        {
            return await new Jnedict().InitAsync(
                path,
                OpenDatabase(File.Open(cache, FileMode.OpenOrCreate), dispose: true));
        }

        public static async Task<Jnedict> CreateAsync(Stream stream, Stream cache)
        {
            return await new Jnedict().InitAsync(
                stream,
                OpenDatabase(cache, dispose: false));
        }

        private static LiteDatabase OpenDatabase(Stream stream, bool dispose)
        {
            return new LiteDatabase(stream, disposeStream: dispose);
        }
    }

    public class JnedictEntry
    {
        public long SequenceNumber { get; }

        public IEnumerable<string> Kanji { get; }

        public IEnumerable<string> Reading { get; }

        public IEnumerable<JnedictTranslation> Translation { get; }

        public JnedictEntry(
            long sequenceNumber,
            IEnumerable<string> kanji,
            IEnumerable<string> reading,
            IEnumerable<JnedictTranslation> translation)
        {
            SequenceNumber = sequenceNumber;
            Kanji = kanji.ToList();
            Reading = reading.ToList();
            Translation = translation.ToList();
        }
    }

    public class JnedictTranslation
    {
        public IEnumerable<JnedictType> Type { get; }

        public IEnumerable<string> Translation { get; }

        public JnedictTranslation(
            IEnumerable<JnedictType> type,
            IEnumerable<string> translation)
        {
            Type = type.ToList();
            Translation = translation.ToList();
        }
    }

    public static class JnedictTypeUtils
    {
        public static Option<JnedictType> FromDescription(string description)
        {
            return mapping.FromDescription(description);
        }

        public static string ToLongString(this JnedictType value)
        {
            return mapping.ToLongString(value);
        }

        private static EnumMapper<JnedictType> mapping = new EnumMapper<JnedictType>();
    }

    public enum JnedictType
    {
        [Description("family or surname")]
        surname,

        [Description("place name")]
        place,

        [Description("unclassified name")]
        unclass,

        [Description("company name")]
        company,

        [Description("product name")]
        product,

        [Description("work of art, literature, music, etc. name")]
        work,

        [Description("male given name or forename")]
        masc,

        [Description("female given name or forename")]
        fem,

        [Description("full name of a particular person")]
        person,

        [Description("given name or forename, gender not specified")]
        given,

        [Description("railway station")]
        station,

        [Description("organization name")]
        organization,

        [Description("old or irregular kana form")]
        ok,
    }

    internal class DbNeDictKeyValue
    {
        public long Id { get; set; }

        public string LookupKey { get; set; }

        [BsonRef("entries")]
        public List<DbNeEntry> Entries { get; set; }
    }

    internal class DbNeEntry
    {
        public long Id { get; set; }

        public long SequenceNumber { get; set; }

        public List<string> Kanji { get; set; }

        public List<string> Reading { get; set; }

        [BsonRef("trans")]
        public List<DbNeTranslation> Translation { get; set; }
    }

    internal class DbNeTranslation
    {
        public long Id { get; set; }

        public List<JnedictType> Type { get; set; }

        public List<string> Detail { get; set; }
    }
}