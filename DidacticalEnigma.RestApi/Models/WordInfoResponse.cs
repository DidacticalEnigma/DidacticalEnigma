using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DidacticalEnigma.RestApi.Models
{
    public class WordInfoResponse
    {
        [Required]
        public IEnumerable<IEnumerable<Models.WordInfo>> WordInformation { get; set; }

        [Required]
        public string Identifier { get; set; }
    }
}
