using DidacticalEnigma.Utils;
using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace DidacticalEnigma.Models
{
    interface IDataSource
    {
        IAsyncEnumerable<FlowDocument> Answer(
            Request request,
            CancellationToken cancellation = default(CancellationToken));

        Task<UpdateResult> UpdateLocalDataSource(
            CancellationToken cancellation = default(CancellationToken));
    }
    public class Request
    {
        public string QueryText { get; }

        public PartOfSpeech PartOfSpeech { get; }

        public Request(string queryText, PartOfSpeech partOfSpeech = PartOfSpeech.Unknown)
        {
            QueryText = queryText;
            PartOfSpeech = partOfSpeech;
        }
    }

    public enum UpdateResult
    {
        NoUpdateNeeded,
        Updated,
        Failure,
        Cancelled
    }
}
