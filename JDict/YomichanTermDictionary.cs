using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JDict.Json;
using JDict.Utils;
using Newtonsoft.Json;

namespace JDict
{
    // provides the programmatic access to dictionary files produced by
    // https://github.com/FooSoft/yomichan-import
    // 
    public class YomichanTermDictionary
    {
        private static readonly Regex termMatcher = new Regex(@"^term_bank_\d+.json$");

        public YomichanTermDictionary(string pathToZip, string cache)
        {
            using (var zip = new ZipFile(pathToZip))
            {
                Init(zip);
            }
        }

        private void Init(IZipFile zip)
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
        }

        public YomichanTermDictionary(IZipFile zipFile, string cache)
        {
            Init(zipFile);
        }

        private TOut With<TResource, TOut>(Func<TResource> resourceFactory, Func<TResource, TOut> resultFactory)
            where TResource : IDisposable
        {
            using (var resource = resourceFactory())
            {
                return resultFactory(resource);
            }
        }
    }
}
