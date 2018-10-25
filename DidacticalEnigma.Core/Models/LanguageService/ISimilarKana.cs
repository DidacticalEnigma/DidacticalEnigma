using System;
using System.Collections.Generic;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    [Obsolete("please use IRelated instead")]
    public interface ISimilarKana
    {
        IEnumerable<CodePoint> FindSimilar(CodePoint point);
    }
}