using NMeCab;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class MeCabIpadic : MeCab<IpadicMeCabEntry>
    {
        public MeCabIpadic(MeCabParam mecabParam) :
            base(mecabParam)
        {

        }

        protected override IpadicMeCabEntry ToEntry(string surface, Option<string> features)
        {
            return new IpadicMeCabEntry(surface, features);
        }
    }
}
