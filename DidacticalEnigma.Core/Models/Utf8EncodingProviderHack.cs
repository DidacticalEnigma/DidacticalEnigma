using System.Text;

namespace DidacticalEnigma.Core.Models
{
    // workaround required for NMeCab and Unidic to work
    // 
    public class Utf8EncodingProviderHack : EncodingProvider
    {
        public override Encoding GetEncoding(string name) => name == "utf8" ? Encoding.UTF8 : null;

        public override Encoding GetEncoding(int codepage) => null;
    }
}
