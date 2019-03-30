using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Documents;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.Formatting;
using Utility.Utils;

namespace DidacticalEnigma.ViewModels
{
    public class DataSourceVM : INotifyPropertyChanged, IDisposable
    {
        private AsyncDataSource dataSource;

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

        public FlowDocument Document => documentRenderer.Render(FormattedResult);

        public string Description => dataSource.Descriptor.Name + " " + dataSource.Kind;

        public string Memento
        {
            get
            {
                var s = dataSource.Descriptor.Guid.ToString();
                if (!string.IsNullOrEmpty(dataSource.Kind))
                    s += "|" + dataSource.Kind;
                return s;
            }
        }


        private bool isProcessing;

        private readonly IFlowDocumentRichFormattingRenderer documentRenderer;

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

        public DataSourceVM(IDataSource dataSource, IFlowDocumentRichFormattingRenderer documentRenderer, string kind = null) :
            this(Task.FromResult(dataSource), dataSource.GetType(), documentRenderer, kind)
        {

        }

        public DataSourceVM(Task<IDataSource> dataSource, Type type, IFlowDocumentRichFormattingRenderer documentRenderer, string kind = null)
        {
            this.documentRenderer = documentRenderer;
            this.dataSource = new AsyncDataSource(dataSource, type, kind);
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