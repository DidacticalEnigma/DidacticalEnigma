using System.Collections.Generic;

namespace DidacticalEnigma.RestApi.InternalServices
{
    public class ParsedText
    {
        public IReadOnlyList<IReadOnlyList<Core.Models.LanguageService.WordInfo>> WordInformation { get; set; }
    }
}
