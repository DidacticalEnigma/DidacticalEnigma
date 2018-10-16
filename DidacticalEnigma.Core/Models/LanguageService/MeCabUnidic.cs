using NMeCab;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class MeCabUnidic : MeCab<UnidicMeCabEntry>
    {
        public MeCabUnidic(MeCabParam mecabParam) :
            base(mecabParam)
        {

        }

        protected override UnidicMeCabEntry ToEntry(string surface, Option<string> features)
        {
            return new UnidicMeCabEntry(surface, features);
        }
    }
}