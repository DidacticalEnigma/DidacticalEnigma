﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DidacticalEnigma.RestApi.Models
{
    public class KanjiLookupResult
    {
        public IReadOnlyCollection<string> Kanji { get; set; }

        public IReadOnlyCollection<string> PossibleRadicals { get; set; }
    }
}
