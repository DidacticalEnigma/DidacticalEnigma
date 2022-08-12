using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using DidacticalEnigma.Core.Models;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.DataSource;
using DidacticalEnigma.Models;
using DidacticalEnigma.Utils;
using DidacticalEnigma.ViewModels;
using DidacticalEnigma.Views;
using Gu.Inject;
using JDict;
using Newtonsoft.Json;
using NMeCab;
using Optional.Collections;
using Sentry;
using Utility.Utils;
using SplashScreen = DidacticalEnigma.Views.SplashScreen;

namespace DidacticalEnigma
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private Kernel kernel;

        private IDisposable sentry;

        public App()
        {
            SetUpCrashReporting(ConfigurationManager.AppSettings["SentryDsn"]);
            Startup += async (sender, args) =>
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dataDir = Path.Combine(baseDir, "Data");
                kernel = Configure(dataDir);
                var window = new MainWindow();
                var splashVm = new SplashScreenVM();
                var splash = new SplashScreen
                {
                    DataContext = splashVm
                };
                IProgress<string> progress = new Progress<string>(s => splashVm.ProgressReport = s);
                splash.Show();
                Encoding.RegisterProvider(new Utf8EncodingProviderHack());
                kernel.Bind<ITextInsertCommand>(() => new TextInsertCommand(window.InsertTextAtCursor));
                kernel.Bind(Dispatcher);
                await Task.Run(() =>
                {
                    Preload(progress);
                });
                
                var vm = kernel.Get<MainWindowVM>();
                window.DataContext = vm;
                window.Show();
                splash.Close();
            };
            Exit += (sender, args) =>
            {
                kernel.Dispose();
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
                    o.Dsn = dsn;
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
            kernel.Get<JMDictLookup>();
            reporter.Report("Initializing JMnedict dictionary (first time may take up to several minutes)");
            kernel.Get<JMNedictLookup>();
            reporter.Report("Initializing MeCab");
            kernel.Get<IMorphologicalAnalyzer<IEntry>>();
            reporter.Report("Initializing other components");
        }

        public static Kernel Configure(string dataDir)
        {
            var kernel = new Kernel();
            
            kernel.Bind(() => KanjiDict.Create(Path.Combine(dataDir, "character", "kanjidic2.xml.gz")));
            kernel.Bind(() => new Kradfile(Path.Combine(dataDir, "character", "kradfile1_plus_2_utf8"), Encoding.UTF8));
            kernel.Bind(() => new Radkfile(Path.Combine(dataDir, "character", "radkfile1_plus_2_utf8"), Encoding.UTF8));
            kernel.Bind(() => JMDictLookup.Create(Path.Combine(dataDir, "dictionaries", "JMdict_e.gz"), Path.Combine(dataDir, "dictionaries", "JMdict_e.cache")));
            kernel.Bind(() => JMNedictLookup.Create(Path.Combine(dataDir, "dictionaries", "JMnedict.xml.gz"), Path.Combine(dataDir, "dictionaries", "JMnedict.xml.cache")));
            kernel.Bind(() =>
                new FrequencyList(Path.Combine(dataDir, "other", "word_form_frequency_list.txt"), Encoding.UTF8));
            kernel.Bind(() => new Tanaka(Path.Combine(dataDir, "corpora", "examples.utf.gz"), Encoding.UTF8));
            kernel.Bind(() => new JESC(Path.Combine(dataDir, "corpora", "jesc_raw"), Encoding.UTF8));
            kernel.Bind(() => new BasicExpressionsCorpus(Path.Combine(dataDir, "corpora", "JEC_basic_sentence_v1-2.csv"), Encoding.UTF8));
            kernel.Bind<IFontResolver>(() => new DefaultFontResolver(Path.Combine(dataDir, "character", "KanjiStrokeOrders")));
            kernel.Bind<IMorphologicalAnalyzer<IpadicEntry>>(() => new MeCabIpadic(new MeCabParam
            {
                DicDir = Path.Combine(dataDir, "mecab", "ipadic"),
                UseMemoryMappedFile = true
            }));
            kernel.Bind<IMorphologicalAnalyzer<UnidicEntry>>(() => new MeCabUnidic(new MeCabParam
            {
                DicDir = Path.Combine(dataDir, "mecab", "unidic"),
                UseMemoryMappedFile = true
            }));
            kernel.Bind<IMorphologicalAnalyzer<IEntry>>(get =>
            {
                // not sure if objects created this way will be disposed twice
                // probably doesn't matter (IDisposable.Dispose's contract says
                // it's not a problem), but still
                try
                {
                    return get.Get<IMorphologicalAnalyzer<UnidicEntry>>();
                }
                catch (Exception)
                {
                    return get.Get<IMorphologicalAnalyzer<IpadicEntry>>();
                }
            });
            kernel.Bind<Models.Settings>(get =>
            {
                try
                {
                    return JsonConvert.DeserializeObject<Models.Settings>(File.ReadAllText("settings.config"));
                }
                catch (FileNotFoundException)
                {
                    return Models.Settings.CreateDefault();
                }
            });
            kernel.Bind(get => new ReplRootModule(
                get.Get<JMDictLookup>(),
                get.Get<JMNedictLookup>(),
                new ReplCorpora(
                    get.Get<BasicExpressionsCorpus>(),
                    get.Get<Tanaka>(),
                    get.Get<JESC>())));
            kernel.Bind(get => new RadicalRemapper(get.Get<Kradfile>(), get.Get<Radkfile>()));
            kernel.Bind(get => EasilyConfusedKana.FromFile(Path.Combine(dataDir, "character", "confused.txt")));
            kernel.Bind<IKanjiProperties, KanjiProperties>();
            kernel.Bind(get => new KanjiProperties(
                get.Get<KanjiDict>(),
                get.Get<Kradfile>(),
                get.Get<Radkfile>(),
                null));
            kernel.Bind(get => new MainWindowVM(
                get.Get<ISentenceParser>(),
                new KanaBoardVM(Path.Combine(dataDir, "character", "hiragana_romaji.txt"), Encoding.UTF8),
                new KanaBoardVM(Path.Combine(dataDir, "character", "katakana_romaji.txt"), Encoding.UTF8),
                get.Get<IEnumerable<UsageDataSourcePreviewVM>>(),
                get.Get<KanjiRadicalLookupControlVM>(),
                get.Get<IRelated>(),
                get.Get<IKanjiProperties>(),
                get.Get<IKanaProperties>(),
                get.Get<IWebBrowser>(),
                () =>
                {
                    var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location)
                        .ProductVersion;
                    return
                        $"Didactical Enigma {version}\n\n{File.ReadAllText(Path.Combine(dataDir, @"about.txt"), Encoding.UTF8)}";
                },
                get.Get<ITextInsertCommand>(),
                get.Get<Models.Settings>(),
                get.Get<ReplVM>(),
                get.Get<DidacticalEnigma.Mem.Client.DidacticalEnigmaMemViewModel>()));
            kernel.Bind(get => new KanjiRadicalLookupControlVM(
                get.Get<KanjiRadicalLookup>(),
                get.Get<IKanjiProperties>(),
                get.Get<IRadicalSearcher>(),
                CreateTextRadicalMappings(get.Get<KanjiRadicalLookup>().AllRadicals, get.Get<RadkfileKanjiAliveCorrelator>())));
            kernel.Bind<IRomaji, ModifiedHepburn>();
            kernel.Bind(get => new ModifiedHepburn(
                get.Get<IMorphologicalAnalyzer<IEntry>>(),
                get.Get<IKanaProperties>()));
            kernel.Bind<IAutoGlosser, AutoGlosserNext>();
            kernel.Bind(get => new AutoGlosserNext(get.Get<ISentenceParser>(), get.Get<JMDictLookup>(), get.Get<IKanaProperties>()));
            kernel.Bind(get => new[] {
                new DataSourceVM(new CharacterDataSource(get.Get<IKanjiProperties>(), get.Get<IKanaProperties>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new CharacterStrokeOrderDataSource(), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new JMDictDataSource(get.Get<JMDictLookup>(), get.Get<IKanaProperties>(), get.Get<JMDictEntrySorter>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new JNeDictDataSource(get.Get<JMNedictLookup>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new JMDictCompactDataSource(get.Get<JMDictLookup>(), get.Get<IKanaProperties>(), get.Get<JMDictEntrySorter>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new VerbConjugationDataSource(get.Get<JMDictLookup>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new WordFrequencyRatingDataSource(get.Get<FrequencyList>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new PartialExpressionJMDictDataSource(get.Get<IdiomDetector>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new JGramDataSource(get.Get<IJGramLookup>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new AutoGlosserDataSource(get.Get<IAutoGlosser>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new CustomNotesDataSource(Path.Combine(dataDir, "custom", "custom_notes.txt")), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new TanakaCorpusFastDataSource(get.Get<Corpus>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new TanakaCorpusDataSource(get.Get<Tanaka>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new BasicExpressionCorpusDataSource(get.Get<BasicExpressionsCorpus>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new PartialWordLookupJMDictDataSource(get.Get<PartialWordLookup>(), get.Get<FrequencyList>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new JESCDataSource(get.Get<JESC>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new RomajiDataSource(get.Get<IRomaji>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new DidacticalEnigmaMemDataSource(get.Get<DidacticalEnigma.Mem.Client.DidacticalEnigmaMemViewModel>().ClientAccessor), get.Get<IFlowDocumentRichFormattingRenderer>())
            }.Concat(get.Get<EpwingDictionaries>().Dictionaries.Select(dict => new DataSourceVM(new EpwingDataSource(dict, get.Get<IKanaProperties>()), get.Get<IFlowDocumentRichFormattingRenderer>(), dict.Revision))));
            kernel.Bind<IKanaProperties, KanaProperties2>();
            kernel.Bind(get => new KanaProperties2(Path.Combine(dataDir, "character", "kana.txt"), Encoding.UTF8));
            kernel.Bind(get => new SimilarKanji(Path.Combine(dataDir, "character", "kanji.tgz_similars.ut8"), Encoding.UTF8));
            kernel.Bind(get => new SentenceParser(get.Get<IMorphologicalAnalyzer<IEntry>>(), get.Get<JMDictLookup>(), get.Get<IKanaProperties>()));
            kernel.Bind<ISentenceParser, SentenceParser>();
            kernel.Bind(get => new JMDictEntrySorter());
            kernel.Bind<IRelated>(get =>
                new CompositeRelated(
                    get.Get<KanaProperties2>(),
                    get.Get<EasilyConfusedKana>(),
                    get.Get<SimilarKanji>()));
            kernel.Bind<IEnumerable<UsageDataSourcePreviewVM>>(get => new []{
                new UsageDataSourcePreviewVM(get.Get<IEnumerable<DataSourceVM>>(), "view.config"),
                new UsageDataSourcePreviewVM(get.Get<IEnumerable<DataSourceVM>>(), "view_2.config"),
                new UsageDataSourcePreviewVM(get.Get<IEnumerable<DataSourceVM>>(), "view_3.config")
            });
            kernel.Bind(get => CreateEpwing(Path.Combine(dataDir, "epwing")));
            kernel.Bind(get => new IdiomDetector(get.Get<JMDictLookup>(), get.Get<IMorphologicalAnalyzer<IpadicEntry>>(), Path.Combine(dataDir, "dictionaries", "idioms.cache")));
            kernel.Bind(get => new PartialWordLookup(get.Get<JMDictLookup>(), get.Get<IRadicalSearcher>(), get.Get<KanjiRadicalLookup>()));
            kernel.Bind(get =>
            {
                using(var reader = File.OpenText(Path.Combine(dataDir, "character", "radkfile1_plus_2_utf8")))
                    return new KanjiRadicalLookup(Radkfile.Parse(reader), get.Get<KanjiDict>());
            });
            kernel.Bind<IWebBrowser>(get => new WebBrowser());
            kernel.Bind<IFlowDocumentRichFormattingRenderer>(get => new FlowDocumentRichFormattingRenderer(get.Get<IFontResolver>(), get.Get<IWebBrowser>()));
            kernel.Bind<IJGramLookup>(get => new JGramLookup(
                Path.Combine(dataDir, "dictionaries", "jgram"),
                Path.Combine(dataDir, "dictionaries", "jgram_lookup"),
                Path.Combine(dataDir, "dictionaries", "jgram.cache")));
            kernel.Bind(get => new RadkfileKanjiAliveCorrelator(Path.Combine(dataDir, "character", "radkfile_kanjilive_correlation_data.txt")));
            kernel.Bind<IRadicalSearcher, RadicalSearcher>();
            kernel.Bind(get => new RadicalSearcher(
                get.Get<KanjiRadicalLookup>().AllRadicals,
                KanjiAliveJapaneseRadicalInformation.Parse(Path.Combine(dataDir, "character", "japanese-radicals.csv")),
                get.Get<RadkfileKanjiAliveCorrelator>()));
            kernel.Bind(get => new Corpus(get.Get<Tanaka>().AllSentences, get.Get<IMorphologicalAnalyzer<IpadicEntry>>(), Path.Combine(dataDir, "corpora", "tanaka.cache")));

            kernel.Bind(get => new DidacticalEnigma.Mem.Client.DidacticalEnigmaMemViewModel());
            return kernel;

            IReadOnlyDictionary<CodePoint, string> CreateTextRadicalMappings(IEnumerable<CodePoint> radicals, IReadOnlyDictionary<int, int> remapper)
            {
                var dict = radicals.ToDictionary(
                    r => r,
                    r => char.ConvertFromUtf32(remapper.GetValueOrNone(r.Utf32).ValueOr(r.Utf32)));
                /*var d = new Dictionary<int, int>
                {
                    {'化', '⺅'},
                    {'刈', '⺉'},
                    {'込', '⻌'},
                    {'汁', '氵'},
                    {'初', '衤'},
                    {'尚', '⺌'},
                    {'買', '罒'},
                    {'犯', '犭'},
                    {'忙', '忄'},
                    {'礼', '礻'},
                    {'个', 131490},
                    {'老', '⺹'},
                    {'扎', '扌'},
                    {'杰', '灬'},
                    {'疔', '疒'},
                    {'禹', '禸'},
                    {'艾', '⺾'},
                    //{'邦', '⻏'},
                    //{'阡', '⻖'},
                    // 并 none available - upside-down ハ
                };*/
                dict[CodePoint.FromInt('邦')] = "邦";
                dict[CodePoint.FromInt('阡')] = "阡";
                dict[CodePoint.FromInt('老')] = "⺹";
                dict[CodePoint.FromInt('并')] = "丷";
                dict[CodePoint.FromInt('乞')] = "𠂉";
                return dict;
            }

            EpwingDictionaries CreateEpwing(string targetPath)
            {
                var dictionaries = new EpwingDictionaries();
                try
                {
                    var zipFiles = new Dictionary<string, ZipFile2>();
                    try
                    {
                        zipFiles = Directory.EnumerateFiles(targetPath, "*.zip")
                            .ToDictionary(path => path, path => new ZipFile2(path));
                        var dicts = zipFiles
                            .Select(file => new YomichanTermDictionary(file.Value, file.Key + ".cache"));
                        foreach (var dict in dicts)
                        {
                            dictionaries.Add(dict);
                        }
                    }
                    finally
                    {
                        foreach(var entry in zipFiles)
                        {
                            entry.Value.Dispose();
                        }
                    }
                }
                catch (DirectoryNotFoundException)
                {

                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(
                        ex.Message + "\nIt was not possible to load the EPWING external dictionary.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

                return dictionaries;
            }
        }
    }
}
