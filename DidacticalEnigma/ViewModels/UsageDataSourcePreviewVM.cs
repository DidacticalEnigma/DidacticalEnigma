using DidacticalEnigma.Models;
using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace DidacticalEnigma.ViewModels
{
    public class DataSourceVM : INotifyPropertyChanged
    {
        private AsyncDataSource dataSource;

        private FlowDocument document;
        public FlowDocument Document
        {
            get => document;
            set
            {
                if (document == value)
                    return;
                document = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsProcessing));
                OnPropertyChanged(nameof(HasFound));
            }
        }

        public DataSourceDescriptor Descriptor => dataSource.Descriptor;

        private bool isProcessing;
        public bool IsProcessing
        {
            get => isProcessing;
            set
            {
                if (isProcessing == value)
                    return;
                isProcessing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasFound));
                OnPropertyChanged(nameof(IsProcessing));
            }
        }

        public bool HasFound => Document != null;

        public async Task Search(Request request)
        {
            IsProcessing = true;
            Document = await dataSource.Answer(request).FirstAsync();
            IsProcessing = false;
        }

        public DataSourceVM(Type type, string path)
        {
            dataSource = new AsyncDataSource(
                () => Task.Run(() => (IDataSource)Activator.CreateInstance(type, path)), type);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class UsageDataSourcePreviewVM : INotifyPropertyChanged
    {
        public ObservableBatchCollection<DataSourceVM> DataSources { get; } = new ObservableBatchCollection<DataSourceVM>();

        private int selectedDataSourceIndex = -1;
        public int SelectedDataSourceIndex
        {
            get => selectedDataSourceIndex;
            set
            {
                if (selectedDataSourceIndex == value)
                    return;

                selectedDataSourceIndex = value;
                OnPropertyChanged();
            }
        }

        private string queryText = null;
        public string QueryText
        {
            get => queryText;
            set
            {
                if (queryText == value)
                    return;

                queryText = value;
                OnPropertyChanged();
                Search(queryText);
            }
        }

        public FlowDocument CurrentFlowDocument
        {
            get
            {
                if (SelectedDataSourceIndex == -1)
                    return null;
                return DataSources[SelectedDataSourceIndex].Document;
            }
        }

        public Task Search(string queryText)
        {
            var tasks = new List<Task>();
            foreach(var dataSource in DataSources)
            {
                tasks.Add(dataSource.Search(new Request(queryText)));
            }
            return Task.WhenAll(tasks);
        }

        public UsageDataSourcePreviewVM(string dataSourcePath)
        {
            DataSources.Add(new DataSourceVM(typeof(TanakaCorpusDataSource), dataSourcePath));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
