using System.Collections.Generic;

namespace DidacticalEnigma.Models
{
    interface ISimilarKana
    {
        IEnumerable<CodePoint> FindSimilar(CodePoint point);
    }
}