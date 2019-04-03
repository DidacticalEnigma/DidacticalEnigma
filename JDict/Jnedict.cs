using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using JDict.Internal.XmlModels;
using Optional;
using Optional.Collections;
using TinyIndex;
using Utility.Utils;
// ReSharper disable InconsistentNaming

namespace JDict
{
    public class Jnedict : IDisposable
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(NeEntry));

        private static readonly Guid Version = new Guid("C4A8A3D3-92F3-4E33-A00F-7BF7DBECDD03");

        private Database database;
        private IReadOnlyDiskArray<JnedictEntry> entries;
        private IReadOnlyDiskArray<KeyValuePair<string, IReadOnlyList<long>>> kvps;

        public void Dispose()
        {
            database.Dispose();
        }

        private Jnedict Init(Stream stream, string cache)
        {
            var entrySerializer = Serializer.ForComposite()
                .With(Serializer.ForLong())
                .With(Serializer.ForReadOnlyList(Serializer.ForStringAsUTF8()))
                .With(Serializer.ForReadOnlyList(Serializer.ForStringAsUTF8()))
                .With(Serializer.ForReadOnlyList(Serializer.ForComposite()
                    .With(Serializer.ForReadOnlyList(Serializer.ForEnum<JnedictType>()))
                    .With(Serializer.ForReadOnlyList(Serializer.ForStringAsUTF8()))
                    .Create()
                    .Mapping(
                        raw => new JnedictTranslation(
                            (IEnumerable<JnedictType>)raw[0],
                            (IEnumerable<string>)raw[1]),
                        obj => new object[]
                            {
                                obj.Type,
                                obj.Translation
                            })))
                .Create()
                .Mapping(
                    raw => new JnedictEntry(
                        (long)raw[0],
                        (IEnumerable<string>)raw[1],
                        (IEnumerable<string>)raw[2],
                        (IEnumerable<JnedictTranslation>)raw[3]),
                    obj => new object[]
                    {
                        obj.SequenceNumber,
                        obj.Kanji,
                        obj.Reading,
                        obj.Translation
                    });


            database = Database.CreateOrOpen(cache, Version)
                .AddIndirectArray(
                    entrySerializer,
                    db => ReadFromXml(stream)
                        .Select(xmlEntry =>
                            new JnedictEntry(
                                xmlEntry.SequenceNumber,
                                (xmlEntry.KanjiElements ?? Array.Empty<KanjiElement>()).Select(k => k.Key),
                                (xmlEntry.ReadingElements ?? Array.Empty<ReadingElement>()).Select(r => r.Reb),
                                (xmlEntry.TranslationalEquivalents ?? Array.Empty<NeTranslationalEquivalent>()).Select(tr =>
                                    new JnedictTranslation(
                                        (tr.Types ?? Array.Empty<string>())
                                        .Select(t => JnedictTypeUtils.FromDescription(t))
                                        .Values(),
                                        (tr.Translation ?? Array.Empty<NeTranslation>())
                                        .Where(t => t.Lang == null || t.Lang == "eng")
                                        .Select(t => t.Text))))),
                    x => x.SequenceNumber)
                .AddIndirectArray(Serializer.ForKeyValuePair(Serializer.ForStringAsUTF8(), Serializer.ForReadOnlyList(Serializer.ForLong())), db =>
                    {
                        IEnumerable<KeyValuePair<long, string>> It(IEnumerable<JnedictEntry> entries)
                        {
                            foreach (var e in entries)
                            {
                                foreach (var r in e.Reading)
                                {
                                    yield return new KeyValuePair<long, string>(e.SequenceNumber, r);
                                }

                                foreach (var k in e.Kanji)
                                {
                                    yield return new KeyValuePair<long, string>(e.SequenceNumber, k);
                                }
                            }
                        }

                        return It(db.Get<JnedictEntry>(0).LinearScan())
                            .GroupBy(kvp => kvp.Value, kvp => kvp.Key)
                            .Select(x => new KeyValuePair<string, IReadOnlyList<long>>(x.Key, x.ToList()));
                    },
                    x => x.Key)
                .Build();

            entries = database.Get<JnedictEntry>(0, new LruCache<long, JnedictEntry>(64));
            kvps = database.Get<KeyValuePair<string, IReadOnlyList<long>>>(1, new LruCache<long, KeyValuePair<string, IReadOnlyList<long>>>(64));

            return this;
        }

        public IEnumerable<JnedictEntry> Lookup(string key)
        {
            var res = kvps.BinarySearch(key, kvp => kvp.Key);
            if (res.id == -1)
            {
                return null;
            }

            return It();

            IEnumerable<JnedictEntry> It()
            {
                var sequenceNumbers = res.element.Value;
                foreach (var sequenceNumber in sequenceNumbers)
                {
                    var searchResult = entries.BinarySearch(sequenceNumber, e => e.SequenceNumber);
                    if (searchResult.id != -1)
                    {
                        yield return searchResult.element;
                    }
                }
            }
        }

        private IEnumerable<NeEntry> ReadFromXml(Stream stream)
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
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "JMnedict")
                    {
                        while (xmlReader.Read())
                        {
                            if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "entry")
                            {
                                using (
                                    var elementReader =
                                        new StringReader(xmlReader.ReadOuterXml()))
                                {
                                    var entry = (NeEntry)serializer.Deserialize(elementReader);
                                    yield return entry;
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task<Jnedict> InitAsync(Stream stream, string cache)
        {
            // TODO: not a lazy way
            await Task.Run(() => Init(stream, cache));
            return this;
        }

        private async Task<Jnedict> InitAsync(string path, string cache)
        {
            using (var file = File.OpenRead(path))
            using (var gzip = new GZipStream(file, CompressionMode.Decompress))
            {
                return await InitAsync(gzip, cache);
            }
        }

        private Jnedict Init(string path, string cache)
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
                cache);
        }

        public static Jnedict Create(Stream stream, string cache)
        {
            return new Jnedict().Init(
                stream,
                cache);
        }

        public static async Task<Jnedict> CreateAsync(string path, string cache)
        {
            return await new Jnedict().InitAsync(
                path,
                cache);
        }

        public static async Task<Jnedict> CreateAsync(Stream stream, string cache)
        {
            return await new Jnedict().InitAsync(
                stream,
                cache);
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
        ok
    }
}