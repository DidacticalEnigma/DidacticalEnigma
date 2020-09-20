using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DidacticalEnigma.RestApi.Models
{
    public class AutoGlossResult
    {
        [Required]
        public IEnumerable<AutoGlossEntry> Entries { get; set; }
    }
}