using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.RestApi.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DidacticalEnigma.RestApi.Controllers
{
    [ApiController]
    [Route("autoGloss")]
    public class AutoGlossController : ControllerBase
    {
        [HttpGet]
        [SwaggerOperation(OperationId = "RunAutomaticGloss")]
        public ActionResult<AutoGlossResult> Gloss(
            [FromQuery] string input,
            [FromServices] IAutoGlosser autoGlosser)
        {
            var result = autoGlosser.Gloss(input);
            return this.Ok(new AutoGlossResult()
            {
                Entries = result
                    .Select(entry => new AutoGlossEntry()
                    {
                        Word = entry.Foreign,
                        Definitions = entry.GlossCandidates
                    })
            });
        }
    }
}