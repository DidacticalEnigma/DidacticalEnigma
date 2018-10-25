using System.Collections.Generic;
using System.Linq;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class CompositeRelated : IRelated
    {
        private readonly IEnumerable<IRelated> relatedProviders;

        public IEnumerable<IGrouping<string, CodePoint>> FindRelated(CodePoint codePoint)
        {
            return relatedProviders.SelectMany(r => r.FindRelated(codePoint));
        }

        public CompositeRelated(IEnumerable<IRelated> relatedProviders)
        {
            this.relatedProviders = relatedProviders;
        }

        public CompositeRelated(params IRelated[] relatedProviders) :
            this(relatedProviders as IEnumerable<IRelated>)
        {

        }
    }
}