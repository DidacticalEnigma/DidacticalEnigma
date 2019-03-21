using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.Formatting;
using Utility.Utils;

namespace DidacticalEnigma.ViewModels
{
    public class DataSourceVM : INotifyPropertyChanged, IDisposable
    {
        private AsyncDataSource dataSource;

        private Request lastRequest;

        private readonly IFontResolver fontResolver;

        private long id = 0;

        private RichFormatting formattedResult = emptyDocument;
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

        private static readonly RichFormatting emptyDocument = new RichFormatting(
            EnumerableExt.OfSingle(
                new TextParagraph(
                    EnumerableExt.OfSingle(new Text("nothing found")))));

        public FlowDocument Document => FlowDocumentRichFormattingRenderer.Render(fontResolver, FormattedResult);

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

        public bool HasFound => FormattedResult != null;

        public async Task Search(Request request, long id)
        {
            try
            {
                if (request == null)
                    return;
                this.id = id;
                IsProcessing = true;
                var result = await dataSource.Answer(request);
                if (this.id > id)
                {
                    return;
                }

                FormattedResult = result.ValueOr(emptyDocument);
                IsProcessing = false;
            }
            finally
            {
                if(request != null)
                    lastRequest = request;
            }
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
    }
}