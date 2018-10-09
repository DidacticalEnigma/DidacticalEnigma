using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using JDict.Internal.XmlModels;
using JDict.Utils;
using LiteDB;
using Optional;
using FileMode = System.IO.FileMode;

namespace JDict
{
    class Jnedict : IDisposable
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(JMNedictRoot));

        private static readonly int Version = 1;

        private LiteCollection<DbDictVersion> version;

        private LiteDatabase db;

        public void Dispose()
        {
            db.Dispose();
        }

        private Jnedict Init(Stream stream, LiteDatabase cache)
        {
            db = cache;
            version = cache.GetCollection<DbDictVersion>("version");
            var versionInfo = version.FindAll().FirstOrDefault();
            if (versionInfo == null ||
                (versionInfo.OriginalFileSize != -1 && stream.CanSeek && stream.Length != versionInfo.OriginalFileSize) ||
                versionInfo.DbVersion != Version)
            {
                var root = ReadFromXml(stream);
                FillDatabase(root);
            }

            return this;
        }

        private void FillDatabase(JMNedictRoot root)
        {
            
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
            {
                return await InitAsync(file, cache);
            }
        }

        private Jnedict Init(string path, LiteDatabase cache)
        {
            using (var file = File.OpenRead(path))
            {
                return Init(file, cache);
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

    public static class JnedictTypeUtils
    {
        public static Option<JnedictType> FromDescription(string description)
        {
            return mapping.FromDescription(description);
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
}