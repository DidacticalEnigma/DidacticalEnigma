using System;
using System.Collections.Async;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public interface IDataSource : IDisposable
    {
        IAsyncEnumerable<RichFormatting> Answer(Request request);

        Task<UpdateResult> UpdateLocalDataSource(
            CancellationToken cancellation = default(CancellationToken));
    }
}
