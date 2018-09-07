using System;
using System.Collections.Async;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Models;

namespace DidacticalEnigma.ViewModels
{
    public class DataSourceVM : INotifyPropertyChanged, IDisposable
    {
        private AsyncDataSource dataSource;

        private readonly IFontResolver fontResolver;

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

        public FlowDocument Document
        {
            get
            {
                if (FormattedResult == null)
                    return emptyDocument;
                else
                    return FlowDocumentRichFormattingRenderer.Render(fontResolver, FormattedResult);
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
            if (!IsUsed || request == null)
                return;
            IsProcessing = true;
            FormattedResult = await dataSource.Answer(request).FirstOrDefaultAsync();
            IsProcessing = false;
        }

        public DataSourceVM(Type type, string path, IFontResolver fontResolver)
        {
            this.fontResolver = fontResolver;
            dataSource = new AsyncDataSource(
                () => Task.Run(() => (IDataSource)Activator.CreateInstance(type, path)), type);
        }

        public DataSourceVM(IDataSource dataSource, IFontResolver fontResolver)
        {
            this.fontResolver = fontResolver;
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
}