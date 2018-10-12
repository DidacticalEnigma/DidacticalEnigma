using DidacticalEnigma.Models;
using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Threading;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Core.Utils;
using JDict;

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
            JMDict jmdict,
            FrequencyList frequencyList,
            Jnedict jnamedict,
            Tanaka tanaka,
            JESC jesc,
            BasicExpressionsCorpus basicExpressions,
            string customNotesPath)
        {
            var fontResolver = new DefaultFontResolver();
            DataSources.Add(new DataSourceVM(new CharacterDataSource(lang), fontResolver));
            DataSources.Add(new DataSourceVM(new JMDictDataSource(jmdict), fontResolver));
            DataSources.Add(new DataSourceVM(new TanakaCorpusDataSource(tanaka), fontResolver));
            DataSources.Add(new DataSourceVM(new CharacterStrokeOrderDataSource(), fontResolver));
            DataSources.Add(new DataSourceVM(new JESCDataSource(jesc), fontResolver));
            DataSources.Add(new DataSourceVM(new BasicExpressionCorpusDataSource(basicExpressions), fontResolver));
            DataSources.Add(new DataSourceVM(new PartialWordLookupJMDictDataSource(jmdict, frequencyList), fontResolver));
            DataSources.Add(new DataSourceVM(new CustomNotesDataSource(customNotesPath), fontResolver));
            DataSources.Add(new DataSourceVM(new VerbConjugationDataSource(jmdict), fontResolver));
            DataSources.Add(new DataSourceVM(new AutoGlosserDataSource(lang, jmdict), fontResolver));
            DataSources.Add(new DataSourceVM(new JNeDictDataSource(jnamedict), fontResolver));

            Func<Element> fac = () => new Leaf(
                () => new DataSourcePreviewVM(this),
                o =>
                {
                    var dataSource = ((DataSourcePreviewVM)o).SelectedDataSource;
                    if (dataSource != null)
                    {
                        dataSource.IsUsed = false;
                    }
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
