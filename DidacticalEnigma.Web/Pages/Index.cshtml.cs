using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.LanguageService;
using Gu.Inject;
using JDict;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NMeCab;
using Optional.Collections;

namespace DidacticalEnigma.Web.Pages
{
    public class IndexModel : PageModel
    {
        private Context context;

        public IndexModel(Context context)
        {
            this.context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (UserInput != null)
            {
                RenderedHtml = await context.RenderAsync(UserInput, CancellationToken.None);
            }
            else
            {
                RenderedHtml = "";
            }

            return Page();
        }

        [BindProperty(SupportsGet = true)]
        public string UserInput { get; set; }


        public string RenderedHtml { get; private set; }
    }
}
