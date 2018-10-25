using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IRelated
    {
        IEnumerable<IGrouping<string, CodePoint>> FindRelated(CodePoint codePoint);
    }
}
