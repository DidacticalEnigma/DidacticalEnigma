using System;
using System.Collections.Generic;
using System.Text;

namespace DidacticalEnigma.Core.Models
{
    // workaround required for NMeCab and Unidic to work
    // 
    public class Utf8EncodingProviderHack : EncodingProvider
    {
        public override Encoding GetEncoding(string name)
        {
            if (name == "utf8")
                return Encoding.UTF8;
            return null;
        }

        public override Encoding GetEncoding(int codepage)
        {
            return null;
        }
    }
}
