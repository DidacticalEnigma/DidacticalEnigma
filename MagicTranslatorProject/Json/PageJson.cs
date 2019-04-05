using System.Collections.Generic;
using Newtonsoft.Json;

namespace MagicTranslatorProject
{
    internal class PageJson
    {
        [JsonProperty("captureId")]
        public int CaptureId { get; set; }

        [JsonProperty("captures")]
        public IList<CaptureJson> Captures { get; set; }
    }
}