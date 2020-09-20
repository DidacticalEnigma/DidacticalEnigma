using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.RestApi.Models;
using Optional;
using Utility.Utils;

namespace DidacticalEnigma.RestApi.InternalServices
{
    public class DataSourceDispatcher
    {
        private readonly Dictionary<string, IDataSource> dataSources;

        private readonly Dictionary<string, DataSourceDescriptor> descriptors;

        public DataSourceDispatcher(IEnumerable<KeyValuePair<string, IDataSource>> dataSources)
        {
            this.dataSources = new Dictionary<string, IDataSource>();
            this.descriptors = new Dictionary<string, DataSourceDescriptor>();
            foreach (var (kind, dataSource) in dataSources)
            {
                if (dataSource.GetType().GetProperty("Descriptor", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) is
                    DataSourceDescriptor descriptor)
                {
                    var dataSourceIdentifier = descriptor.Guid.ToString() + (kind != null ? "|" + kind : "");
                    this.dataSources.Add(dataSourceIdentifier, dataSource);
                    this.descriptors.Add(dataSourceIdentifier, descriptor);
                }
            }
        }

        public IEnumerable<DataSourceInformation> DataSourceIdentifiers =>
            descriptors
                .Select(kvp =>
                {
                    var kind = kvp.Key.Split("|").ElementAtOrDefault(1);
                    kind = kind != null ? $" ({kind})" : "";
                    return new DataSourceInformation()
                    {
                        Identifier = kvp.Key,
                        FriendlyName = kvp.Value.Name + kind
                    };
                });

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
