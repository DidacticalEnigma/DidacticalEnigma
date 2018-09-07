using System.Collections.Generic;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    interface ISimilarKana
    {
        IEnumerable<CodePoint> FindSimilar(CodePoint point);
    }
}