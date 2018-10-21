using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Core.Utils;
using JDict;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DidacticalEnigma.ViewModels
{
    public class UsageDataSourcePreviewVM : INotifyPropertyChanged, IDisposable
    {
        public ObservableBatchCollection<DataSourceVM> DataSources { get; } = new ObservableBatchCollection<DataSourceVM>();

        public Root Root { get; }

        private Request request = null;
        public Request Request
        {
            get => request;
            set
            {
                if (request == value)
                    return;

                request = value;
                OnPropertyChanged();
                Task.Run(() => Search(request));
            }
        }

        private long id = 0;

        public Task Search(Request req)
        {
            var tasks = new List<Task>();
            var id = Interlocked.Increment(ref this.id);
            foreach(var dataSource in DataSources)
            {
                tasks.Add(dataSource.Search(req, id));
            }
            return Task.WhenAll(tasks);
        }

        public UsageDataSourcePreviewVM(
            ILanguageService lang,
            IEnumerable<DataSourceVM> dataSources)
        {
            DataSources.AddRange(dataSources);
            Leaf Fac() => new Leaf(() => new DataSourcePreviewVM(this), o =>
            {
                var dataSource = ((DataSourcePreviewVM) o).SelectedDataSource;
                if (dataSource != null)
                {
                    dataSource.IsUsed = false;
                }
            });

            try
            {
                var root = JsonConvert.DeserializeObject<Root>(File.ReadAllText("view.config"),
                    new ElementCreationConverter(Fac));
                Root = root;
            }
            catch (FileNotFoundException)
            {
                Root = new Root(Fac);
                // being lazy
                (Root.Tree as Leaf).VSplit.Execute(null);
                (((Root.Tree as Split).First as Leaf).Content as DataSourcePreviewVM).SelectedDataSourceIndex = 0;
                (((Root.Tree as Split).Second as Leaf).Content as DataSourcePreviewVM).SelectedDataSourceIndex = 1;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            foreach(var dataSource in DataSources)
            {
                dataSource.Dispose();
            }
            File.WriteAllText("view.config", JsonConvert.SerializeObject(Root));
        }
    }
}
