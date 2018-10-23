using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using DidacticalEnigma.Core.Models;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.ViewModels;
using DidacticalEnigma.Views;
using Gu.Inject;
using JDict;
using NMeCab;
using SplashScreen = DidacticalEnigma.Views.SplashScreen;

namespace DidacticalEnigma
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Kernel Kernel;

        private IReadOnlyCollection<EpwingDataSource> epwing;

        public App()
        {
            var splash = new SplashScreen();
            splash.Show();
            Encoding.RegisterProvider(new Utf8EncodingProviderHack());
            Configure();
            Startup += (sender, args) =>
            {
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
                foreach (var ep in epwing)
                {
                    ep.Dispose();
                }
            };
        }

        private void Configure()
        {
            var kernel = new Kernel();
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = Path.Combine(baseDir, "Data");
            epwing = CreateEpwingDataSources(Path.Combine(dataDir, "epwing")).ToList();
            kernel.BindFactory(() => JDict.KanjiDict.Create(Path.Combine(dataDir, "character", "kanjidic2.xml.gz")));
            kernel.BindFactory(() => new Kradfile(Path.Combine(dataDir, "character", "kradfile1_plus_2_utf8"), Encoding.UTF8));
            kernel.BindFactory(() => new Radkfile(Path.Combine(dataDir, "character", "radkfile1_plus_2_utf8"), Encoding.UTF8));
            kernel.BindFactory(() => JDict.JMDict.Create(Path.Combine(dataDir, "dictionaries", "JMdict_e.gz"), Path.Combine(dataDir, "dictionaries", "JMdict_e.cache")));
            kernel.BindFactory(() => JDict.Jnedict.Create(Path.Combine(dataDir, "dictionaries", "JMnedict.xml.gz"), Path.Combine(dataDir, "dictionaries", "JMnedict.xml.cache")));
            kernel.BindFactory(() =>
                new FrequencyList(Path.Combine(dataDir, "other", "word_form_frequency_list.txt"), Encoding.UTF8));
            kernel.BindFactory(() => new KanaProperties(
                hiraganaPath: Path.Combine(dataDir, "character", "hiragana_romaji.txt"),
                katakanaPath: Path.Combine(dataDir, "character", "katakana_romaji.txt"),
                hiraganaKatakanaPath: Path.Combine(dataDir, "character", "hiragana_katakana.txt"),
                complexPath: Path.Combine(dataDir, "character", "kana_related.txt"),
                encoding: Encoding.UTF8));
            kernel.BindFactory(() => new Tanaka(Path.Combine(dataDir, "corpora", "examples.utf.gz"), Encoding.UTF8));
            kernel.BindFactory(() => new JESC(Path.Combine(dataDir, "corpora", "jesc_raw"), Encoding.UTF8));
            kernel.BindFactory(() => new BasicExpressionsCorpus(Path.Combine(dataDir, "corpora", "JEC_basic_sentence_v1-2.csv"), Encoding.UTF8));
            kernel.BindFactory<IFontResolver>(() => new DefaultFontResolver(Path.Combine(dataDir, "character", "KanjiStrokeOrders")));
            kernel.BindFactory<IMorphologicalAnalyzer<IEntry>>(() => new MeCabIpadic(new MeCabParam
            {
                DicDir = Path.Combine(dataDir, "mecab", "ipadic"),
            }));
            kernel.BindFactory(get => new RadicalRemapper(get.Get<Kradfile>(), get.Get<Radkfile>()));
            kernel.BindFactory<ILanguageService>(get => new LanguageService(
                get.Get<IMorphologicalAnalyzer<IEntry>>(),
                EasilyConfusedKana.FromFile(Path.Combine(dataDir, "character", "confused.txt")),
                get.Get<Kradfile>(),
                get.Get<Radkfile>(),
                get.Get<KanjiDict>(),
                get.Get<KanaProperties>(),
                get.Get<RadicalRemapper>()));
            kernel.BindFactory(get => new MainWindowVM(
                get.Get<ILanguageService>(),
                new KanaBoardVM(Path.Combine(dataDir, "character", "hiragana_romaji.txt"), Encoding.UTF8, get.Get<ILanguageService>()),
                new KanaBoardVM(Path.Combine(dataDir, "character", "katakana_romaji.txt"), Encoding.UTF8, get.Get<ILanguageService>()),
                get.Get<UsageDataSourcePreviewVM>(),
                get.Get<KanjiRadicalLookupControlVM>(),
                () => File.ReadAllText(Path.Combine(dataDir, @"about.txt"), Encoding.UTF8)));
            kernel.BindFactory(get => new KanjiRadicalLookupControlVM(
                get.Get<ILanguageService>(),
                get.Get<KanjiDict>(),
                get.Get<RadicalRemapper>()));
            kernel.BindFactory<IEnumerable<DataSourceVM>>(get => new[] {
                new DataSourceVM(new CharacterDataSource(get.Get<ILanguageService>()), get.Get<IFontResolver>()),
                new DataSourceVM(new CharacterStrokeOrderDataSource(), get.Get<IFontResolver>()),
                new DataSourceVM(new JMDictDataSource(get.Get<JMDict>()), get.Get<IFontResolver>()),
                new DataSourceVM(new JNeDictDataSource(get.Get<Jnedict>()), get.Get<IFontResolver>()),
                new DataSourceVM(new VerbConjugationDataSource(get.Get<JMDict>()), get.Get<IFontResolver>()),
                new DataSourceVM(new AutoGlosserDataSource(get.Get<ILanguageService>(), get.Get<JMDict>()), get.Get<IFontResolver>()),
                new DataSourceVM(new CustomNotesDataSource(Path.Combine(dataDir, "custom", "custom_notes.txt")), get.Get<IFontResolver>()),
                new DataSourceVM(new TanakaCorpusDataSource(get.Get<Tanaka>()), get.Get<IFontResolver>()),
                new DataSourceVM(new BasicExpressionCorpusDataSource(get.Get<BasicExpressionsCorpus>()), get.Get<IFontResolver>()),
                new DataSourceVM(new PartialWordLookupJMDictDataSource(get.Get<JMDict>(), get.Get<FrequencyList>()), get.Get<IFontResolver>()),
                new DataSourceVM(new JESCDataSource(get.Get<JESC>()), get.Get<IFontResolver>())
            }.Concat(epwing.Select(source => new DataSourceVM(source, get.Get<IFontResolver>()))));
            kernel.BindFactory(get => new UsageDataSourcePreviewVM(
                get.Get<ILanguageService>(),
                get.Get<IEnumerable<DataSourceVM>>()));
            Kernel = kernel;
            

            IEnumerable<EpwingDataSource> CreateEpwingDataSources(string targetPath)
            {
                try
                {
                    return Directory.EnumerateFiles(targetPath, "*.zip")
                        .Take(1) // we need to ensure our capability to handle multiple data sources with the same GUID
                        .Select(file => new YomichanTermDictionary(file, file + ".cache"))
                        .Select(dataSource => new EpwingDataSource(dataSource));
                }
                catch (DirectoryNotFoundException e)
                {
                    return Enumerable.Empty<EpwingDataSource>();
                }
            }
        }
    }
}
