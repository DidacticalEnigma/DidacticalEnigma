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

        internal CaptureContext(CaptureJson json, Func<CaptureJson, ModificationResult> saveAction, Translation translation)
        {
            this.saveAction = saveAction;
            this.Translation = translation;
        }

        public DidacticalEnigma.Core.Models.Project.Translation Translation { get; private set; }

        public ModificationResult Modify(DidacticalEnigma.Core.Models.Project.Translation translation)
        {
            var r = saveAction(((Translation)translation).Capture);
            if(r.IsSuccessful)
                Translation = translation;
            return r;
        }

        public IEnumerable<ITranslationContext> Children => Enumerable.Empty<ITranslationContext>();

        public RichFormatting Render(RenderingVerbosity verbosity)
        {
            throw new NotImplementedException();
        }
    }
}