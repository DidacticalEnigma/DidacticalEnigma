﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DidacticalEnigma.RestApi.Models
{
    public class DataSourceInformation
    {
        [Required]
        public string Identifier { get; set; }
        
        [Required]
        public string FriendlyName { get; set; }
    }
}
