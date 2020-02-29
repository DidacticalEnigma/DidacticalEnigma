using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DidacticalEnigma.RestApi.Controllers
{
    [Route("radicals")]
    public class RadicalsController : Controller
    {
        [HttpGet("list")]
        [SwaggerOperation(OperationId = "ListRadicals")]
        public IEnumerable<string> ListRadicals(
            [FromServices] IKanjiRadicalLookup radicalLookup)
        {
            return radicalLookup.AllRadicals.Select(r => r.ToString());
        }
    }
}
