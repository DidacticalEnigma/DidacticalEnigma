using System;
using System.Collections.Async;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;

namespace DidacticalEnigma.Core.Models.DataSources
{
    // asynchronously initializing wrapper over a IDataSource
    // not implementing the IDataSource because it purposefully violates the contract
    public class AsyncDataSource
    {
        private readonly Task<IDataSource> dataSource;

        private bool disposed = false;

        public enum InitializationState
        {
            InProgress,
            Success,
            Failure
        }

        public DataSourceDescriptor Descriptor { get; }

        public InitializationState State
        {
            get
            {
                if (dataSource.IsFaulted)
                    return InitializationState.Failure;
                if (dataSource.IsCanceled)
                    return InitializationState.Failure;
                if (dataSource.IsCompleted)
                    return InitializationState.Success;
                return InitializationState.InProgress;
            }

        }

        public IAsyncEnumerable<RichFormatting> Answer(Request request)
        {
            return new AsyncEnumerable<RichFormatting>(async yield =>
            {
                var dataSource = await this.dataSource.ConfigureAwait(false);
                var results = dataSource.Answer(request);
                await results.ForEachAsync(async result =>
                {
                    await yield.ReturnAsync(result).ConfigureAwait(false);
                }, yield.CancellationToken).ConfigureAwait(false);
            });
        }

        public async void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            if (State == InitializationState.Success)
            {
                dataSource.Result.Dispose();
                return;
            }
            // if it's still initializing, then we wait for a while
            if (State == InitializationState.InProgress)
                await Task.Delay(5000);
            // then dispose it if it's initialized
            if (State == InitializationState.Success)
                dataSource.Result.Dispose();
            // otherwise fuck it
        }

        public async Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            var dataSource = await this.dataSource.ConfigureAwait(false);
            return await dataSource.UpdateLocalDataSource(cancellation);
        }

        public AsyncDataSource(Func<Task<IDataSource>> dataSource, Type type)
        {
            if (type.GetProperty("Descriptor", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) is DataSourceDescriptor descriptor)
            {
                Descriptor = descriptor;
                this.dataSource = dataSource();
            }
            else
            {
                this.dataSource = Task.FromException<IDataSource>(new InvalidOperationException("the data source doesn't provide a descriptor"));
            }
        }
    }
}
