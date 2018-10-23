using NMeCab;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class MeCabIpadic : MeCab<IpadicEntry>
    {
        public MeCabIpadic(MeCabParam mecabParam) :
            base(mecabParam)
        {

        }

        protected override IpadicEntry ToEntry(string surface, Option<string> features)
        {
            return new IpadicEntry(surface, features);
        }
    }
}
