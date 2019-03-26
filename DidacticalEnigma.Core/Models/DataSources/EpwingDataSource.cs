using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.LanguageService;
using JDict;
using Optional;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class EpwingDataSource : IDataSource
    {
        private readonly YomichanTermDictionary dict;
        private readonly IKanaProperties kana;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("0C0999F9-F361-46D3-8E24-BA3A7CA669E7"),
            "Epwing",
            "...",
            null);

        public Task<Option<RichFormatting>> Answer(Request request)
        {
            return DictUtils.Lookup(
                request,
                t => dict.Lookup(t),
                r => GreedyLookup(r),
                kana);
        }

        private (IEnumerable<YomichanTermDictionary.Entry> entry, string word) GreedyLookup(Request request, int backOffCountStart = 5)
        {
            if (request.SubsequentWords == null)
                return (null, null);

            return DictUtils.GreedyLookup(s => dict.Lookup(s), request.SubsequentWords, backOffCountStart);
        }

        public void Dispose()
        {

        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default)
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public EpwingDataSource(YomichanTermDictionary dict, IKanaProperties kana)
        {
            this.dict = dict;
            this.kana = kana;
        }
    }
}
