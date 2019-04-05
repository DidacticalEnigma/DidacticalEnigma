using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MagicTranslatorProject
{
    internal class CaptureJson
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("translation")]
        public string Translation { get; set; }

        [JsonProperty("notes")]
        public IEnumerable<IReadOnlyList<string>> Notes { get; set; }

        [JsonProperty("character")]
        public CharacterType Character { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [UsedImplicitly]
        [JsonExtensionData]
        private readonly Dictionary<string, JToken> extensionData = new Dictionary<string, JToken>();
    }
}