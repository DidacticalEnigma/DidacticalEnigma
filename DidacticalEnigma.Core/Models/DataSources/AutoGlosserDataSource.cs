using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.LanguageService;
using Optional;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class AutoGlosserDataSource : IDataSource
    {
        private readonly IAutoGlosser autoglosser;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("76EAA6CD-2FD7-4691-9B07-E9FE907F89E9"),
            "AutoGlosser",
            "NOTE: This functionality is completely untested and may result in horribly broken glosses",
            null);

        public Task<Option<RichFormatting>> Answer(Request request, CancellationToken token)
        {
            var rich = new RichFormatting();
            var text = request.AllText();
            var glosses = autoglosser.Gloss(text);
            rich.Paragraphs.Add(
                new TextParagraph(EnumerableExt.OfSingle(new Text(Descriptor.AcknowledgementText))));
            var s = string.Join("\n", glosses.Select(gloss => $"- {gloss.Foreign}:\n{string.Join("\n", gloss.GlossCandidates.Select(c => $"    - {c}"))}"));
            rich.Paragraphs.Add(new TextParagraph(EnumerableExt.OfSingle(new Text(s))));
            return Task.FromResult(Option.Some(rich));
        }

        public void Dispose()
        {

        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default)
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public AutoGlosserDataSource(IAutoGlosser autoglosser)
        {
            this.autoglosser = autoglosser;
        }
    }
}