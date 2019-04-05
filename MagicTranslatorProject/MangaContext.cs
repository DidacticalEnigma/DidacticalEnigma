using System;
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
        public bool IsAddSupported(DidacticalEnigma.Core.Models.Project.ITranslation translation)
        {
            return false;
        }

        public ModificationResult Modify(DidacticalEnigma.Core.Models.Project.ITranslation translation)
        {
            return ModificationResult.WithUnsupported("translations are only available on page level");
        }

        public IEnumerable<VolumeContext> Children { get; }

        IEnumerable<ITranslationContext> ITranslationContext.Children => Children;

        public IEnumerable<DidacticalEnigma.Core.Models.Project.ITranslation> Translations => Enumerable.Empty<DidacticalEnigma.Core.Models.Project.ITranslation>();

        public void Add(DidacticalEnigma.Core.Models.Project.ITranslation translation)
        {
            throw new InvalidOperationException();
        }

        public RichFormatting Render()
        {
            throw new NotImplementedException();
        }

        public RichFormatting Render(ITranslation translation)
        {
            throw new InvalidOperationException();
        }

        private static readonly Regex chapterNumberMatcher = new Regex("^ch([0-9]{3})$");

        internal MangaContext(string rootPath, IEnumerable<int> volumeNumbers)
        {
            RootPath = rootPath;
            IdNameMapping = new DualDictionary<long, string>(JsonConvert.DeserializeObject<CharactersJson>(
                File.ReadAllText(Path.Combine(rootPath, "character", "characters.json")))
                .Characters
                .ToDictionary(c => c.Id, c => c.Name));
            Children = volumeNumbers
                .Select(vol =>
                {
                    var chapterNumbers = new DirectoryInfo(Path.Combine(rootPath, $"vol{vol:D2}"))
                        .EnumerateDirectories()
                        .OrderBy(dir => dir.Name)
                        .Select(dir => chapterNumberMatcher.Match(dir.Name))
                        .Where(match => match.Success)
                        .Select(match => int.Parse(match.Groups[1].Value));
                    return new VolumeContext(this, vol, chapterNumbers);
                });
        }

        internal IReadOnlyDualDictionary<long, string> IdNameMapping { get; }

        public string RootPath { get; }
    }

    public class CharactersJson
    {
        [JsonProperty("characterId")]
        public int CharacterId { get; set; }

        [JsonProperty("characters")]
        public IEnumerable<CharacterJson> Characters { get; set; }
    }

    public class CharacterJson
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}