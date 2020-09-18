using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.RestApi.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Optional;
using Optional.Unsafe;
using Utility.Utils;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DidacticalEnigma.RestApi.Controllers
{
    [Route("radicals")]
    public class RadicalsController : Controller
    {
        [HttpGet("list")]
        [SwaggerOperation(OperationId = "ListRadicals")]
        public ActionResult<ListRadicalsResult> ListRadicals(
            [FromServices] IKanjiRadicalLookup radicalLookup)
        {
            return this.Ok(new ListRadicalsResult
            {
                PossibleRadicals = radicalLookup.AllRadicals
                    .Select(r => r.ToString())
                    .ToList(),
                SortingCriteria = radicalLookup.SortingCriteria
                    .Select(r => r.Description)
                    .ToList()
            });
        }

        [HttpGet("select")]
        [SwaggerOperation(OperationId = "SelectRadicals")]
        public ActionResult<KanjiLookupResult> SelectRadicals(
            [FromQuery(Name = "query")] string query,
            [FromQuery(Name = "sort")] string sort,
            [FromServices] IKanjiRadicalLookup radicalLookup,
            [FromServices] IRadicalSearcher radicalSearcher)
        {
            query ??= "";
            var sortingCriteriaIndexOpt = sort == null
                ? Option.Some<int>(0)
                : radicalLookup.SortingCriteria
                    .FindIndexOrNone(criterion => criterion.Description == sort);

            if(!sortingCriteriaIndexOpt.HasValue)
            {
                return this.BadRequest("invalid sorting criterion");
            }

            var sortingCriteriaIndex = sortingCriteriaIndexOpt.ValueOrFailure();

            var radicalSearchResults = radicalSearcher.Search(query);
            var radicals = radicalSearchResults
                .Select(result => result.Radical)
                .ToList();

            if(radicals.Count == 0)
            {
                return this.Ok(new KanjiLookupResult
                {
                    Kanji = Array.Empty<string>(),
                    PossibleRadicals = radicalLookup.AllRadicals
                        .Select(r => r.ToString())
                        .ToList(),
                    UsedRadicals = new Dictionary<string, string>()
                });
            }

            var result = radicalLookup.SelectRadical(radicals, sortingCriteriaIndex);
            return this.Ok(new KanjiLookupResult
            {
                Kanji = result.Kanji
                    .Select(k => k.ToString())
                    .ToList(),
                PossibleRadicals = result.PossibleRadicals
                    .Where(r => r.Value)
                    .Select(r => r.Key.ToString())
                    .Concat(radicals.Select(x => x.ToString()))
                    .ToList(),
                UsedRadicals = radicalSearchResults
                    .DistinctBy(r => r.Text)
                    .ToDictionary(r => r.Text, r => r.Radical.ToString())
            });
        }
    }
}
