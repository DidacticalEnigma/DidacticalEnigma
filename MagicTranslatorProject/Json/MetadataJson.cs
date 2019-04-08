using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MagicTranslatorProject.Json
{
    class MetadataJson
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("structure")]
        public StructureJson Structure { get; set; }
    }

    class StructureJson
    {
        [JsonProperty("volume")]
        public string Volume { get; set; }

        [JsonProperty("chapter")]
        public string Chapter { get; set; }

        [JsonProperty("page")]
        public string Page { get; set; }

        [JsonProperty("raw")]
        public string Raw { get; set; }

        [JsonProperty("translated")]
        public string Translated { get; set; }

        [JsonProperty("capture")]
        public string Capture { get; set; }

        [JsonProperty("save")]
        public string Save { get; set; }
    }
}
