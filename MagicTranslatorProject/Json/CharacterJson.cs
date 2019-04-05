using Newtonsoft.Json;

namespace MagicTranslatorProject
{
    public class CharacterJson
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}