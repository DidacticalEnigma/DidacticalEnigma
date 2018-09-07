using System;
using System.Collections.Async;
using System.Threading;
using System.Threading.Tasks;

namespace DidacticalEnigma.Models
{
    public interface IDataSource : IDisposable
    {
        IAsyncEnumerable<RichFormatting> Answer(Request request);

        Task<UpdateResult> UpdateLocalDataSource(
            CancellationToken cancellation = default(CancellationToken));
    }
}
