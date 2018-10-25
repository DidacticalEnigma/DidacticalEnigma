using System.Collections.Generic;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface ISimilarKana
    {
        IEnumerable<CodePoint> FindSimilar(CodePoint point);
    }
}