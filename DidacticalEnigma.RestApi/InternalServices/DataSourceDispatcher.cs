using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.Formatting;
using Optional;
using Utility.Utils;

namespace DidacticalEnigma.RestApi.InternalServices
{
    public class DataSourceDispatcher
    {
        private readonly Dictionary<string, IDataSource> dataSources;

        public DataSourceDispatcher(IEnumerable<IDataSource> dataSources)
        {
            this.dataSources = dataSources.ToDictionary(
                d => Guid.NewGuid().ToString(),
                d => d);
        }

        public IEnumerable<string> DataSourceIdentifiers => dataSources.Keys;

        public async Task<Option<RichFormatting>> GetAnswer(
            string dataSourceIdentifier,
            Request request,
            CancellationToken token = default)
        {
            var result = await (dataSources
                .GetValueOrDefault(dataSourceIdentifier)
                ?.Answer(request, token)
                ?? Task.FromResult(Option.None<RichFormatting>()));
            return result;
        }
    }
}
