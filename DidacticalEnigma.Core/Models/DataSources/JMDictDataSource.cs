using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using JDict;
using Optional;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class JMDictDataSource : IDataSource
    {
        private readonly JMDict jdict;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("ED1B840C-B2A8-4018-87B0-D5FC64A1ABC8"),
            "JMDict",
            "The data JMdict by Electronic Dictionary Research and Development Group",
            new Uri("http://www.edrdg.org/jmdict/j_jmdict.html"));

        public Task<Option<RichFormatting>> Answer(Request request)
        {
            return DictUtils.Lookup(
                request,
                t => jdict.Lookup(t),
                r => GreedyLookup(r));
        }

        private (IEnumerable<JMDictEntry> entry, string word) GreedyLookup(Request request, int backOffCountStart = 5)
        {
            if (request.SubsequentWords == null)
                return (null, null);

            return DictUtils.GreedyLookup(s => jdict.Lookup(s), request.SubsequentWords, backOffCountStart);
        }

        public void Dispose()
        {

        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public JMDictDataSource(JMDict jdict)
        {
            this.jdict = jdict;
        }
    }
}