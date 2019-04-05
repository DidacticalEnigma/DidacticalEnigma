using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.Project;
using Newtonsoft.Json;
using Utility.Utils;

namespace MagicTranslatorProject
{
    public class MangaContext : ITranslationContext<VolumeContext>
    {
        public IEnumerable<VolumeContext> Children
        {
            get
            {
                if (volumes.TryGetTarget(out var v))
                {
                    return v;
                }
                else
                {
                    v = Load();
                    volumes.SetTarget(v);
                    return v;
                }
            }
        }

        IEnumerable<ITranslationContext> ITranslationContext.Children => Children;

        private WeakReference<IReadOnlyCollection<VolumeContext>> volumes = new WeakReference<IReadOnlyCollection<VolumeContext>>(null);

        private static readonly Regex volumeNumberMatcher = new Regex("^vol([0-9]{2})$");

        internal MangaContext(string rootPath)
        {
            RootPath = rootPath;
            IdNameMapping = new DualDictionary<long, string>(JsonConvert.DeserializeObject<CharactersJson>(
                File.ReadAllText(Path.Combine(rootPath, "character", "characters.json")))
                .Characters
                .ToDictionary(c => c.Id, c => c.Name));
        }

        private IReadOnlyCollection<VolumeContext> Load()
        {
            return new DirectoryInfo(RootPath)
                .EnumerateDirectories()
                .Select(dir => volumeNumberMatcher.Match(dir.Name))
                .Where(match => match.Success)
                .Select(match => int.Parse(match.Groups[1].Value))
                .OrderBy(vol => vol)
                .Select(vol =>
                {
                    return new VolumeContext(this, vol);
                })
                .ToList();
        }

        internal IReadOnlyDualDictionary<long, string> IdNameMapping { get; }

        internal Guid Map(int volumeNumber, int chapterNumber, int pageNumber, long captureId)
        {
            return guidMap.GetOrAdd(
                (volumeNumber, chapterNumber, pageNumber, captureId),
                x => Guid.NewGuid());
        }

        private ConcurrentDictionary<(int volumeNumber, int chapterNumber, int pageNumber, long captureId), Guid> guidMap =
            new ConcurrentDictionary<(int volumeNumber, int chapterNumber, int pageNumber, long captureId), Guid>();

        public string RootPath { get; }

        public string Name
        {
            get
            {
                // TODO
                return "manga";
            }
        }
    }
}