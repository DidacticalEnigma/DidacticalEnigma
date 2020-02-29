using System.Collections.Generic;
using DidacticalEnigma.Core.Models.LanguageService;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Swashbuckle.AspNetCore.Annotations;

namespace DidacticalEnigma.RestApi.Controllers
{
    [ApiController]
    [Route("wordInfo")]
    public class WordInfoController : ControllerBase
    {

        [HttpGet]
        [SwaggerOperation(OperationId = "GetWordInformation")]
        public IEnumerable<IEnumerable<Models.WordInfo>> Get(
            [FromQuery] string fullText,
            [FromServices] ISentenceParser parser)
        {
            return parser
                .BreakIntoSentences(fullText)
                .Select(sentence => sentence.
                    Select(word => new Models.WordInfo()
                    {
                        DictionaryForm = word.DictionaryForm,
                        Reading = word.Reading,
                        Text = word.RawWord
                    }));
        }
    }
}
