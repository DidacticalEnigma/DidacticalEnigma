using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.RestApi.InternalServices;
using DidacticalEnigma.RestApi.Models;
using Microsoft.AspNetCore.Mvc;
using Optional.Unsafe;
using Swashbuckle.AspNetCore.Annotations;
using Utility.Utils;

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
        public async Task<ActionResult<Dictionary<string, DataSourceParseResponse>>> RequestInformation(
            [FromBody] DataSourceParseRequest request,
            [FromServices] IStash<ParsedText> stash,
            [FromServices] RichFormattingRenderer renderer,
            [FromServices] DataSourceDispatcher dataSourceDispatcher)
        {
            var parsedTextOpt = stash.Get(request.Id);
            if (!parsedTextOpt.HasValue)
            {
                return this.Conflict();
            }

            var parsedText = parsedTextOpt.ValueOrFailure();
            var dataSourceRequest = DataSourceRequestFromParsedText(parsedText, request.Position);

            var requestedDataSources = request.RequestedDataSources.Distinct();
            var result = new Dictionary<string, DataSourceParseResponse>();
            foreach (var identifier in requestedDataSources)
            {
                var answerOpt = await dataSourceDispatcher.GetAnswer(
                    identifier,
                    dataSourceRequest);
                result[identifier] = new DataSourceParseResponse()
                {
                    Context = answerOpt
                        .Map(a => renderer.Render(a).OuterXml)
                        .ValueOr(null as string)
                };
            }

            return result;
        }

        private Request DataSourceRequestFromParsedText(ParsedText text, int position)
        {
            var (wordPosition, outerIndex, innerIndex) = text.GetIndicesAtPosition(position);
            var wordInfo = text.WordInformation[outerIndex][innerIndex];

            return new Request(
                char.ConvertFromUtf32(wordInfo.RawWord.AsCodePoints().ElementAt(position - wordPosition)),
                wordInfo,
                wordInfo.RawWord,
                () => text.FullText,
                SubsequentWords());

            IEnumerable<string> SubsequentWords()
            {
                for (int i = outerIndex; i < text.WordInformation.Count; i++)
                {
                    for (int j = (i == outerIndex) ? innerIndex : 0; j < text.WordInformation[i].Count; j++)
                    {
                        yield return text.WordInformation[i][j].RawWord;
                    }
                }
            }
        }
    }
}