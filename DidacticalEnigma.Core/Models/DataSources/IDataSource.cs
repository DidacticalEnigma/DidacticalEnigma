using System;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using Optional;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public interface IDataSource : IDisposable
    {
        Task<Option<RichFormatting>> Answer(Request request, CancellationToken token);

        Task<UpdateResult> UpdateLocalDataSource(
            CancellationToken cancellation = default(CancellationToken));
    }
}
