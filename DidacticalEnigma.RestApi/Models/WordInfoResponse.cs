using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DidacticalEnigma.RestApi.Models
{
    public class WordInfoResponse
    {
        public IEnumerable<IEnumerable<Models.WordInfo>> WordInformation { get; set; }

        public string Identifier { get; set; }
    }
}
