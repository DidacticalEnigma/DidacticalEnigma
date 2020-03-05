using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DidacticalEnigma.RestApi.Models
{
    public class DataSourceParseRequest
    {
        public IReadOnlyCollection<string> RequestedDataSources { get; set; }

        public string Id { get; set; }

        public int Position { get; set; }
    }
}
