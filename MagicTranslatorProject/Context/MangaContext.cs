using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.Project;
using MagicTranslatorProject.Json;
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

        public RichFormatting Render(RenderingVerbosity verbosity)
        {
            throw new NotImplementedException();
        }

        public string ShortDescription => Name;

        IEnumerable<ITranslationContext> ITranslationContext.Children => Children;

        private WeakReference<IReadOnlyCollection<VolumeContext>> volumes = new WeakReference<IReadOnlyCollection<VolumeContext>>(null);

        internal MangaContext(MetadataJson metadata, string rootPath, ProjectDirectoryListingProvider listing)
        {
            this.listing = listing;
            this.metadata = metadata;
            RootPath = rootPath;
            IdNameMapping = new DualDictionary<long, string>(JsonConvert.DeserializeObject<CharactersJson>(
                File.ReadAllText(listing.GetCharactersPath()))
                .Characters
                .ToDictionary(c => c.Id, c => c.Name));
        }

        private readonly MetadataJson metadata;

        private IReadOnlyCollection<VolumeContext> Load()
        {
            return listing.EnumerateVolumes()
                .Select(vol =>
                {
                    return new VolumeContext(this, vol, listing);
                })
                .ToList();
        }

        internal IReadOnlyDualDictionary<long, string> IdNameMapping { get; }

        internal Guid Map(PageId page, long captureId)
        {
            return guidMap.GetOrAdd(
                (page, captureId),
                x => Guid.NewGuid());
        }

        private ConcurrentDictionary<(PageId page, long captureId), Guid> guidMap =
            new ConcurrentDictionary<(PageId page, long captureId), Guid>();

        private readonly ProjectDirectoryListingProvider listing;

        public string RootPath { get; }

        public string Name => metadata.Name;
    }
}