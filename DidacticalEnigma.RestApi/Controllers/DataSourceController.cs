using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.RestApi.InternalServices;
using DidacticalEnigma.RestApi.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DidacticalEnigma.RestApi.Controllers
{
    [ApiController]
    [Route("dataSource")]
    public class DataSourceController : ControllerBase
    {
        [HttpGet("list")]
        [SwaggerOperation(OperationId = "ListDataSources")]
        public IEnumerable<DataSourceInformation> List(
            [FromServices] DataSourceDispatcher dataSourceDispatcher)
        {
            return dataSourceDispatcher.DataSourceIdentifiers
                .Select(id => new DataSourceInformation
                {
                    Identifier = id
                });
        }

        [HttpPost("request")]
        [SwaggerOperation(OperationId = "RequestInformationFromDataSources")]
        public async Task<Dictionary<string, DataSourceParseResponse>> RequestInformation(
            [FromBody] DataSourceParseRequest request,
            [FromServices] RichFormattingRenderer renderer,
            [FromServices] DataSourceDispatcher dataSourceDispatcher)
        {
            var result = request.RequestedDataSources
                .ToDictionary(s => s, s => null as DataSourceParseResponse);
            foreach (var identifier in result.Keys)
            {
                var answerOpt = await dataSourceDispatcher.GetAnswer(identifier);
                result[identifier] = new DataSourceParseResponse()
                {
                    Context = answerOpt
                        .Map(a => renderer.Render(a).OuterXml)
                        .ValueOr(null as string)
                };
            }

            return result;
        }
    }
}