using DidacticalEnigma.Models;
using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Threading;

namespace DidacticalEnigma.ViewModels
{
    public class DataSourceVM : INotifyPropertyChanged, IDisposable
    {
        private AsyncDataSource dataSource;

        private RichFormatting formattedResult;
        public RichFormatting FormattedResult
        {
            get => formattedResult;
            set
            {
                if (formattedResult == value)
                    return;
                formattedResult = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsProcessing));
                OnPropertyChanged(nameof(HasFound));
                OnPropertyChanged(nameof(Document));
            }
        }

        private FlowDocument emptyDocument = new FlowDocument(new System.Windows.Documents.Paragraph(new Run("nothing found")));

        public FlowDocument Document => FormattedResult?.Render() ?? emptyDocument;

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
            }
        }

        private bool isUsed;
        public bool IsUsed
        {
            get => isUsed;
            set
            {
                if (isUsed == value)
                    return;
                isUsed = value;
                OnPropertyChanged();
            }
        }

        public Visibility Visible
        {
            get
            {
                if (isUsed)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public bool HasFound => FormattedResult != null;

        public async Task Search(Request request)
        {
            IsProcessing = true;
            FormattedResult = await dataSource.Answer(request).FirstOrDefaultAsync();
            IsProcessing = false;
        }

        public DataSourceVM(Type type, string path)
        {
            dataSource = new AsyncDataSource(
                () => Task.Run(() => (IDataSource)Activator.CreateInstance(type, path)), type);
        }

        public DataSourceVM(IDataSource dataSource)
        {
            this.dataSource = new AsyncDataSource(
                () => Task.FromResult(dataSource), dataSource.GetType());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            dataSource.Dispose();
        }

        public override string ToString()
        {
            return Descriptor.Name;
        }
    }

    public class DataSourcePreviewVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DataSourcePreviewVM(UsageDataSourcePreviewVM parent, int index = -1)
        {
            Parent = parent;
            SelectedDataSourceIndex = index;
            if(index != -1)
            {
                SelectedDataSource = Parent.DataSources[index];
            }
        }

        public UsageDataSourcePreviewVM Parent { get; }

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

        private DataSourceVM selectedDataSource = null;
        public DataSourceVM SelectedDataSource
        {
            get => selectedDataSource;
            set
            {
                if (selectedDataSource == value)
                    return;
                var oldSource = selectedDataSource;
                selectedDataSource = value;
                OnPropertyChanged();
                value.IsUsed = true;
                if(oldSource != null)
                    oldSource.IsUsed = false;
            }
        }
    }

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

            Func<Element> fac = () => new Leaf(
                () => new DataSourcePreviewVM(this),
                o =>
                {
                    ((DataSourcePreviewVM)o).SelectedDataSource.IsUsed = false;
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
