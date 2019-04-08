using System;
using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.Project;

namespace MagicTranslatorProject
{
    public class CaptureContext : IEditableTranslation
    {
        private readonly Func<CaptureJson, ModificationResult> saveAction;
        private readonly PageContext pageContext;
        private readonly CaptureJson json;

        internal CaptureContext(
            PageContext pageContext,
            CaptureJson json,
            Func<CaptureJson, ModificationResult> saveAction,
            Translation translation)
        {
            this.pageContext = pageContext;
            this.json = json;
            this.saveAction = saveAction;
            this.Translation = translation;
        }

        public DidacticalEnigma.Core.Models.Project.Translation Translation { get; private set; }

        public ModificationResult Modify(DidacticalEnigma.Core.Models.Project.Translation translation)
        {
            var input = ((Translation) translation);
            var r = saveAction(input.With(json).Capture);
            if(r.IsSuccessful)
                Translation = translation;
            return r;
        }

        public IEnumerable<ITranslationContext> Children => Enumerable.Empty<ITranslationContext>();

        public RichFormatting Render(RenderingVerbosity verbosity)
        {
            throw new NotImplementedException();
        }

        public string ShortDescription => $"{pageContext.ShortDescription}. {json.Character}";
    }
}