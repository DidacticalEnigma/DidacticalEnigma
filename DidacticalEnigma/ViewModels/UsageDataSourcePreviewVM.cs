using DidacticalEnigma.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Threading;
using DidacticalEnigma.Models.DataSources;

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

        public Task Search(Request req)
        {
            var tasks = new List<Task>();
            foreach(var dataSource in DataSources)
            {
                tasks.Add(dataSource.Search(req));
            }
            return Task.WhenAll(tasks);
        }

        public UsageDataSourcePreviewVM(ILanguageService lang, string dataSourcePath)
        {
            DataSources.Add(new DataSourceVM(new CharacterDataSource(lang)));
            DataSources.Add(new DataSourceVM(typeof(JMDictDataSource), dataSourcePath));
            DataSources.Add(new DataSourceVM(typeof(TanakaCorpusDataSource), dataSourcePath));
            DataSources.Add(new DataSourceVM(typeof(CharacterStrokeOrderDataSource), dataSourcePath));
            DataSources.Add(new DataSourceVM(typeof(JESCDataSource), dataSourcePath));

            Func<Element> fac = () => new Leaf(
                () => new DataSourcePreviewVM(this),
                o =>
                {
                    var dataSource = ((DataSourcePreviewVM)o).SelectedDataSource;
                    if(dataSource != null)
                        dataSource.IsUsed = false;
                });
            Root = new Root(fac);
            // being lazy
            (Root.Tree as Leaf).VSplit.Execute(null);
            (((Root.Tree as Split).First as Leaf).Content as DataSourcePreviewVM).SelectedDataSourceIndex = 0;
            (((Root.Tree as Split).Second as Leaf).Content as DataSourcePreviewVM).SelectedDataSourceIndex = 1;
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
        }
    }
}
