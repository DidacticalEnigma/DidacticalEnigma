using System.Collections.Generic;
using DidacticalEnigma.Core.Models.DataSources;
using Newtonsoft.Json;

namespace DidacticalEnigma.CLI.Common
{
    public class ExpectedGloss
    {
        [JsonProperty("word")]
        public string Word { get; set; }

        [JsonProperty("definitions")]
        public IEnumerable<string> Definitions { get; set; }
    }
}
