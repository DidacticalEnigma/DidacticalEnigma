using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DidacticalEnigma.RestApi.Models
{
    public class WordInfo
    {
        [Required]
        public string Text { get; set; }

        [Required]
        public string DictionaryForm { get; set; }

        [Required]
        public string Reading { get; set; }
    }
}
