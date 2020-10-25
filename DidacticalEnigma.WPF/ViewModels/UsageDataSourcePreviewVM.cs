using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.DataSources;
using Newtonsoft.Json;
using Utility.Utils;

namespace DidacticalEnigma.ViewModels
{
    public class Selectable<T> : INotifyPropertyChanged, IDisposable
        where T : IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public T Entity { get; }

        public Selectable(T entity)
        {
            Entity = entity;
        }

        private bool selected;
        public bool Selected
        {
            get => selected;
            set
            {
                if (value == selected)
                    return;

                selected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected)));
            }
        }

        public void Dispose()
        {
            Entity.Dispose();
        }
    }

    public class UsageDataSourcePreviewVM : INotifyPropertyChanged, IDisposable
    {
        private readonly string configPath;

        public ObservableBatchCollection<Selectable<DataSourceVM>> DataSources { get; } = new ObservableBatchCollection<Selectable<DataSourceVM>>();

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
                tokenSource.Cancel();
                var t = new CancellationTokenSource();
                Task.Run(() => Search(request, t.Token), t.Token);
                tokenSource = t;
            }
        }

        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        private long id = 0;

        public Task Search(Request req, CancellationToken token)
        {
            var tasks = new List<Task>();
            var id = Interlocked.Increment(ref this.id);
            foreach(var dataSource in DataSources)
            {
                if (token.IsCancellationRequested)
                    break;
                if(dataSource.Selected)
                    tasks.Add(dataSource.Entity.Search(req, id, token));
            }
            return Task.WhenAll(tasks);
        }

        public UsageDataSourcePreviewVM(IEnumerable<DataSourceVM> dataSources, string configPath)
        {
            this.configPath = configPath;
            DataSources.AddRange(dataSources.Select(d => new Selectable<DataSourceVM>(d)));
            Leaf Fac() => new Leaf(() => new DataSourcePreviewVM(this), o =>
            {
                var dataSource = ((DataSourcePreviewVM) o).SelectedDataSource;
                if (dataSource != null)
                {
                    dataSource.Selected = false;
                }
            });

            try
            {
                var root = JsonConvert.DeserializeObject<Root>(File.ReadAllText(configPath),
                    new ElementCreationConverter(Fac));
                Root = root;
            }
            catch (FileNotFoundException)
            {
                Root = new Root(Fac);
                // being lazy
                (Root.Tree as Leaf).VSplit.Execute(null);
                (((Root.Tree as Split).First as Leaf).Content as DataSourcePreviewVM).SelectedDataSourceIndex = 0;
                (((Root.Tree as Split).Second as Leaf).Content as DataSourcePreviewVM).SelectedDataSourceIndex = 2;
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
            File.WriteAllText(configPath, JsonConvert.SerializeObject(Root));
        }
    }
}
