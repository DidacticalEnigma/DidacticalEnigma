using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DidacticalEnigma.Core.Models;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Utils;
using DidacticalEnigma.ViewModels;
using DidacticalEnigma.Views;
using Gu.Inject;
using JDict;
using NMeCab;
using Sentry;
using SplashScreen = DidacticalEnigma.Views.SplashScreen;

namespace DidacticalEnigma
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Kernel Kernel;

        private IDisposable sentry;

        public App()
        {
            SetUpCrashReporting(ConfigurationManager.AppSettings["SentryDsn"]);
            Startup += async (sender, args) =>
            {
                Configure();
                var splashVm = new SplashScreenVM();
                var splash = new SplashScreen()
                {
                    DataContext = splashVm
                };
                IProgress<string> progress = new Progress<string>(s => splashVm.ProgressReport = s);
                splash.Show();
                Encoding.RegisterProvider(new Utf8EncodingProviderHack());
                await Task.Run(() =>
                {
                    Preload(progress);
                });
                var vm = Kernel.Get<MainWindowVM>();
                var window = new MainWindow()
                {
                    DataContext = vm
                };
                window.Show();
                splash.Close();
            };
            Exit += (sender, args) =>
            {
                Kernel.Dispose();
            };
        }

        // does nothing if dsn is null
        private void SetUpCrashReporting(string dsn)
        {
            if (string.IsNullOrWhiteSpace(dsn))
                return;
            Startup += (sender, args) =>
            {
                sentry = SentrySdk.Init(o =>
                {
                    o.Dsn = new Dsn(dsn);
                    o.AttachStacktrace = true;
                });
            };
            Exit += (sender, args) =>
            {
                sentry.Dispose();
            };
        }

        public void Preload(IProgress<string> reporter)
        {
            reporter.Report("Initializing JMdict dictionary (first time may take up to several minutes)");
            var jdict = Kernel.Get<JMDict>();
            reporter.Report("Initializing JMnedict dictionary (first time may take up to several minutes)");
            var jnedict = Kernel.Get<Jnedict>();
            reporter.Report("Initializing MeCab");
            var mecab = Kernel.Get<IMorphologicalAnalyzer<IEntry>>();
            reporter.Report("Initializing other components");
        }

        private void Configure()
        {
            var kernel = new Kernel();
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = Path.Combine(baseDir, "Data");
            kernel.BindFactory(() => JDict.KanjiDict.Create(Path.Combine(dataDir, "character", "kanjidic2.xml.gz")));
            kernel.BindFactory(() => new Kradfile(Path.Combine(dataDir, "character", "kradfile1_plus_2_utf8"), Encoding.UTF8));
            kernel.BindFactory(() => new Radkfile(Path.Combine(dataDir, "character", "radkfile1_plus_2_utf8"), Encoding.UTF8));
            kernel.BindFactory(() => JDict.JMDict.Create(Path.Combine(dataDir, "dictionaries", "JMdict_e.gz"), Path.Combine(dataDir, "dictionaries", "JMdict_e.cache")));
            kernel.BindFactory(() => JDict.Jnedict.Create(Path.Combine(dataDir, "dictionaries", "JMnedict.xml.gz"), Path.Combine(dataDir, "dictionaries", "JMnedict.xml.cache")));
            kernel.BindFactory(() =>
                new FrequencyList(Path.Combine(dataDir, "other", "word_form_frequency_list.txt"), Encoding.UTF8));
            kernel.BindFactory(() => new Tanaka(Path.Combine(dataDir, "corpora", "examples.utf.gz"), Encoding.UTF8));
            kernel.BindFactory(() => new JESC(Path.Combine(dataDir, "corpora", "jesc_raw"), Encoding.UTF8));
            kernel.BindFactory(() => new BasicExpressionsCorpus(Path.Combine(dataDir, "corpora", "JEC_basic_sentence_v1-2.csv"), Encoding.UTF8));
            kernel.BindFactory<IFontResolver>(() => new DefaultFontResolver(Path.Combine(dataDir, "character", "KanjiStrokeOrders")));
            kernel.BindFactory<IMorphologicalAnalyzer<IpadicEntry>>(() => new MeCabIpadic(new MeCabParam
            {
                DicDir = Path.Combine(dataDir, "mecab", "ipadic"),
            }));
            kernel.Bind<IMorphologicalAnalyzer<IEntry>, IMorphologicalAnalyzer<IpadicEntry>>();
            kernel.BindFactory(get => new RadicalRemapper(get.Get<Kradfile>(), get.Get<Radkfile>()));
            kernel.BindFactory(get => EasilyConfusedKana.FromFile(Path.Combine(dataDir, "character", "confused.txt")));
            kernel.Bind<IKanjiProperties, KanjiProperties>();
            kernel.BindFactory(get => new KanjiProperties(
                get.Get<KanjiDict>(),
                get.Get<Kradfile>(),
                get.Get<Radkfile>(),
                null));
            kernel.BindFactory(get => new MainWindowVM(
                get.Get<IMorphologicalAnalyzer<IEntry>>(),
                new KanaBoardVM(Path.Combine(dataDir, "character", "hiragana_romaji.txt"), Encoding.UTF8),
                new KanaBoardVM(Path.Combine(dataDir, "character", "katakana_romaji.txt"), Encoding.UTF8),
                get.Get<IEnumerable<UsageDataSourcePreviewVM>>(),
                get.Get<KanjiRadicalLookupControlVM>(),
                get.Get<IRelated>(),
                get.Get<IKanjiProperties>(),
                get.Get<IKanaProperties>(),
                get.Get<IWebBrowser>(),
                () => File.ReadAllText(Path.Combine(dataDir, @"about.txt"), Encoding.UTF8)));
            kernel.BindFactory(get => new KanjiRadicalLookupControlVM(
                get.Get<KanjiRadicalLookup>(),
                get.Get<IKanjiProperties>()));
            kernel.Bind<IRomaji, ModifiedHepburn>();
            kernel.BindFactory(get => new ModifiedHepburn(
                get.Get<IMorphologicalAnalyzer<IEntry>>(),
                get.Get<IKanaProperties>()));
            kernel.BindFactory(get => new AutoGlosser(get.Get<IMorphologicalAnalyzer<IEntry>>(), get.Get<JMDict>()));
            kernel.BindFactory<IEnumerable<DataSourceVM>>(get => new[] {
                new DataSourceVM(new CharacterDataSource(get.Get<IKanjiProperties>(), get.Get<IKanaProperties>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new CharacterStrokeOrderDataSource(), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new JMDictDataSource(get.Get<JMDict>(), get.Get<IKanaProperties>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new JNeDictDataSource(get.Get<Jnedict>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new VerbConjugationDataSource(get.Get<JMDict>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new PartialExpressionJMDictDataSource(get.Get<IdiomDetector>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new JGramDataSource(get.Get<IJGramLookup>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new AutoGlosserDataSource(get.Get<AutoGlosser>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new CustomNotesDataSource(Path.Combine(dataDir, "custom", "custom_notes.txt")), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new TanakaCorpusDataSource(get.Get<Tanaka>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new BasicExpressionCorpusDataSource(get.Get<BasicExpressionsCorpus>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new PartialWordLookupJMDictDataSource(get.Get<PartialWordLookup>(), get.Get<FrequencyList>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new JESCDataSource(get.Get<JESC>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new RomajiDataSource(get.Get<IRomaji>()), get.Get<IFlowDocumentRichFormattingRenderer>())
            }.Concat(get.Get<EpwingDictionaries>().Dictionaries.Select(dict => new DataSourceVM(new EpwingDataSource(dict, get.Get<IKanaProperties>()), get.Get<IFlowDocumentRichFormattingRenderer>()))));
            kernel.Bind<IKanaProperties, KanaProperties2>();
            kernel.BindFactory(get => new KanaProperties2(Path.Combine(dataDir, "character", "kana.txt"), Encoding.UTF8));
            kernel.BindFactory(get => new SimilarKanji(Path.Combine(dataDir, "character", "kanji.tgz_similars.ut8"), Encoding.UTF8));
            kernel.BindFactory<IRelated>(get =>
                new CompositeRelated(
                    get.Get<KanaProperties2>(),
                    get.Get<EasilyConfusedKana>(),
                    get.Get<SimilarKanji>()));
            kernel.BindFactory<IEnumerable<UsageDataSourcePreviewVM>>(get => new []{
                new UsageDataSourcePreviewVM(get.Get<IEnumerable<DataSourceVM>>(), "view.config"),
                new UsageDataSourcePreviewVM(get.Get<IEnumerable<DataSourceVM>>(), "view_2.config"),
                new UsageDataSourcePreviewVM(get.Get<IEnumerable<DataSourceVM>>(), "view_3.config")
            });
            kernel.BindFactory(get => CreateEpwing(Path.Combine(dataDir, "epwing")));
            kernel.BindFactory(get => new IdiomDetector(get.Get<JMDict>(), get.Get<IMorphologicalAnalyzer<IpadicEntry>>(), Path.Combine(dataDir, "dictionaries", "idioms.cache")));
            kernel.BindFactory(get => new PartialWordLookup(get.Get<JMDict>()));
            kernel.BindFactory(get =>
            {
                using(var reader = File.OpenText(Path.Combine(dataDir, "character", "radkfile1_plus_2_utf8")))
                    return new KanjiRadicalLookup(Radkfile.Parse(reader), get.Get<KanjiDict>());
            });
            kernel.BindFactory<IWebBrowser>(get => new WebBrowser());
            kernel.BindFactory<IFlowDocumentRichFormattingRenderer>(get => new FlowDocumentRichFormattingRenderer(get.Get<IFontResolver>(), get.Get<IWebBrowser>()));
            kernel.BindFactory<IJGramLookup>(get => new JGramLookup(
                Path.Combine(dataDir, "dictionaries", "jgram"),
                Path.Combine(dataDir, "dictionaries", "jgram_lookup"),
                Path.Combine(dataDir, "dictionaries", "jgram.cache")));
            Kernel = kernel;
            

            EpwingDictionaries CreateEpwing(string targetPath)
            {
                var dictionaries = new EpwingDictionaries();
                try
                {
                    var dicts = Directory.EnumerateFiles(targetPath, "*.zip")
                        .Take(1) // we need to ensure our capability to handle multiple data sources with the same GUID
                        .Select(file => new YomichanTermDictionary(file, file + ".cache"));
                    foreach (var dict in dicts)
                    {
                        dictionaries.Add(dict);
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    
                }

                return dictionaries;
            }
        }
    }
}
