using NMeCab;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class MeCabUnidic : MeCab<UnidicEntry>
    {
        public MeCabUnidic(MeCabParam mecabParam) :
            base(mecabParam)
        {

        }

        protected override UnidicEntry ToEntry(string surface, Option<string> features)
        {
            return new UnidicEntry(surface, features);
        }
    }
}