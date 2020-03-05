using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.Formatting;
using Optional;

namespace DidacticalEnigma.RestApi.InternalServices
{
    public class DataSourceDispatcher
    {
        public DataSourceDispatcher(IEnumerable<IDataSource> dataSources)
        {
            
        }

        public IEnumerable<string> DataSourceIdentifiers { get; }

        public Task<Option<RichFormatting>> GetAnswer(string dataSourceIdentifier)
        {
            return Task.FromResult(Option.Some<RichFormatting>(new RichFormatting()));
        }
    }
}
