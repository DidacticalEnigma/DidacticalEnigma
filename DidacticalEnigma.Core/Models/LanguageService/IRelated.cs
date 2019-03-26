using System.Collections.Generic;
using System.Linq;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IRelated
    {
        IEnumerable<IGrouping<string, CodePoint>> FindRelated(CodePoint codePoint);
    }
}
