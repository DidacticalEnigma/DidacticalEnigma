using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma;
using DidacticalEnigma.Core.Models;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.LanguageService;
using Gu.Inject;
using JDict;
using NMeCab;
using NUnit.Framework;
using Optional.Collections;

namespace AutomatedTests
{
    [TestFixture]
    class PartialWordLookupTests
    {
        [Explicit]
        [Test]
        public void T()
        {
            var kernel = Configure(TestDataPaths.BaseDir);
            var partial = kernel.Get<PartialWordLookup>();
            var entries = partial.LookupWords("*く太陽!").ToList();
        }

        public static Kernel Configure(string dataDir)
        {
            var kernel = new Kernel();

            kernel.BindFactory(() => KanjiDict.Create(Path.Combine(dataDir, "character", "kanjidic2.xml.gz")));
            kernel.BindFactory(() => new JDict.Kradfile(Path.Combine(dataDir, "character", "kradfile1_plus_2_utf8"), Encoding.UTF8));
            kernel.BindFactory(() => new Radkfile(Path.Combine(dataDir, "character", "radkfile1_plus_2_utf8"), Encoding.UTF8));
            kernel.BindFactory(() => JMDictLookup.Create(Path.Combine(dataDir, "dictionaries", "JMdict_e.gz"), Path.Combine(dataDir, "dictionaries", "JMdict_e.cache")));
            kernel.BindFactory(() => Jnedict.Create(Path.Combine(dataDir, "dictionaries", "JMnedict.xml.gz"), Path.Combine(dataDir, "dictionaries", "JMnedict.xml.cache")));
            kernel.BindFactory(() =>
                new FrequencyList(Path.Combine(dataDir, "other", "word_form_frequency_list.txt"), Encoding.UTF8));
            kernel.BindFactory(() => new Tanaka(Path.Combine(dataDir, "corpora", "examples.utf.gz"), Encoding.UTF8));
            kernel.BindFactory(() => new JESC(Path.Combine(dataDir, "corpora", "jesc_raw"), Encoding.UTF8));
            kernel.BindFactory(() => new BasicExpressionsCorpus(Path.Combine(dataDir, "corpora", "JEC_basic_sentence_v1-2.csv"), Encoding.UTF8));
            kernel.BindFactory<IMorphologicalAnalyzer<IpadicEntry>>(() => new MeCabIpadic(new MeCabParam
            {
                DicDir = Path.Combine(dataDir, "mecab", "ipadic")
            }));
            kernel.Bind<IMorphologicalAnalyzer<IEntry>, IMorphologicalAnalyzer<IpadicEntry>>();
            kernel.BindFactory(get => new RadicalRemapper(get.Get<JDict.Kradfile>(), get.Get<Radkfile>()));
            kernel.BindFactory(get => EasilyConfusedKana.FromFile(Path.Combine(dataDir, "character", "confused.txt")));
            kernel.Bind<IKanjiProperties, KanjiProperties>();
            kernel.BindFactory(get => new KanjiProperties(
                get.Get<KanjiDict>(),
                get.Get<JDict.Kradfile>(),
                get.Get<Radkfile>(),
                null));
            kernel.Bind<IRomaji, ModifiedHepburn>();
            kernel.BindFactory(get => new ModifiedHepburn(
                get.Get<IMorphologicalAnalyzer<IEntry>>(),
                get.Get<IKanaProperties>()));
            kernel.BindFactory(get => new AutoGlosserNext(get.Get<ISentenceParser>(), get.Get<JMDictLookup>(), get.Get<IKanaProperties>()));
            kernel.Bind<IKanaProperties, KanaProperties2>();
            kernel.BindFactory(get => new KanaProperties2(Path.Combine(dataDir, "character", "kana.txt"), Encoding.UTF8));
            kernel.BindFactory(get => new SimilarKanji(Path.Combine(dataDir, "character", "kanji.tgz_similars.ut8"), Encoding.UTF8));
            kernel.BindFactory<IRelated>(get =>
                new CompositeRelated(
                    get.Get<KanaProperties2>(),
                    get.Get<EasilyConfusedKana>(),
                    get.Get<SimilarKanji>()));
            kernel.BindFactory(get => CreateEpwing(Path.Combine(dataDir, "epwing")));
            kernel.BindFactory(get => new IdiomDetector(get.Get<JMDictLookup>(), get.Get<IMorphologicalAnalyzer<IpadicEntry>>(), Path.Combine(dataDir, "dictionaries", "idioms.cache")));
            kernel.BindFactory(get => new PartialWordLookup(get.Get<JMDictLookup>(), get.Get<IRadicalSearcher>(), get.Get<KanjiRadicalLookup>()));
            kernel.BindFactory(get =>
            {
                using (var reader = File.OpenText(Path.Combine(dataDir, "character", "radkfile1_plus_2_utf8")))
                    return new KanjiRadicalLookup(Radkfile.Parse(reader), get.Get<KanjiDict>());
            });

            kernel.BindFactory<IJGramLookup>(get => new JGramLookup(
                Path.Combine(dataDir, "dictionaries", "jgram"),
                Path.Combine(dataDir, "dictionaries", "jgram_lookup"),
                Path.Combine(dataDir, "dictionaries", "jgram.cache")));
            kernel.BindFactory(get => new RadkfileKanjiAliveCorrelator(Path.Combine(dataDir, "character", "radkfile_kanjilive_correlation_data.txt")));
            kernel.Bind<IRadicalSearcher, RadicalSearcher>();
            kernel.BindFactory(get => new SentenceParser(get.Get<IMorphologicalAnalyzer<IEntry>>(), get.Get<JMDictLookup>()));
            kernel.Bind<ISentenceParser, SentenceParser>();
            kernel.BindFactory(get => new RadicalSearcher(
                get.Get<KanjiRadicalLookup>().AllRadicals,
                KanjiAliveJapaneseRadicalInformation.Parse(Path.Combine(dataDir, "character", "japanese-radicals.csv")),
                get.Get<RadkfileKanjiAliveCorrelator>()));
            kernel.BindFactory(get => new Corpus(get.Get<Tanaka>().AllSentences, get.Get<IMorphologicalAnalyzer<IpadicEntry>>(), Path.Combine(dataDir, "corpora", "tanaka.cache")));

            /*
            kernel.BindFactory<IFontResolver>(() => new DefaultFontResolver(Path.Combine(dataDir, "character", "KanjiStrokeOrders")));
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
                () => File.ReadAllText(Path.Combine(dataDir, @"about.txt"), Encoding.UTF8),
                get.Get<ITextInsertCommand>()));
            kernel.BindFactory(get => new KanjiRadicalLookupControlVM(
                get.Get<KanjiRadicalLookup>(),
                get.Get<IKanjiProperties>(),
                get.Get<IRadicalSearcher>(),
                CreateTextRadicalMappings(get.Get<KanjiRadicalLookup>().AllRadicals, get.Get<RadkfileKanjiAliveCorrelator>())));
             kernel.BindFactory(get => new[] {
                new DataSourceVM(new CharacterDataSource(get.Get<IKanjiProperties>(), get.Get<IKanaProperties>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new CharacterStrokeOrderDataSource(), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new JMDictDataSource(get.Get<JMDict>(), get.Get<IKanaProperties>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new JNeDictDataSource(get.Get<Jnedict>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new VerbConjugationDataSource(get.Get<JMDict>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new WordFrequencyRatingDataSource(get.Get<FrequencyList>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new PartialExpressionJMDictDataSource(get.Get<IdiomDetector>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new JGramDataSource(get.Get<IJGramLookup>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new AutoGlosserDataSource(get.Get<AutoGlosser>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new CustomNotesDataSource(Path.Combine(dataDir, "custom", "custom_notes.txt")), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new TanakaCorpusFastDataSource(get.Get<Corpus>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new TanakaCorpusDataSource(get.Get<Tanaka>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new BasicExpressionCorpusDataSource(get.Get<BasicExpressionsCorpus>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new PartialWordLookupJMDictDataSource(get.Get<PartialWordLookup>(), get.Get<FrequencyList>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new JESCDataSource(get.Get<JESC>()), get.Get<IFlowDocumentRichFormattingRenderer>()),
                new DataSourceVM(new RomajiDataSource(get.Get<IRomaji>()), get.Get<IFlowDocumentRichFormattingRenderer>())
            }.Concat(get.Get<EpwingDictionaries>().Dictionaries.Select(dict => new DataSourceVM(new EpwingDataSource(dict, get.Get<IKanaProperties>()), get.Get<IFlowDocumentRichFormattingRenderer>(), dict.Revision))));
            kernel.BindFactory<IEnumerable<UsageDataSourcePreviewVM>>(get => new[]{
                new UsageDataSourcePreviewVM(get.Get<IEnumerable<DataSourceVM>>(), "view.config"),
                new UsageDataSourcePreviewVM(get.Get<IEnumerable<DataSourceVM>>(), "view_2.config"),
                new UsageDataSourcePreviewVM(get.Get<IEnumerable<DataSourceVM>>(), "view_3.config")
            });
            kernel.BindFactory<IWebBrowser>(get => new WebBrowser());
            kernel.BindFactory<IFlowDocumentRichFormattingRenderer>(get => new FlowDocumentRichFormattingRenderer(get.Get<IFontResolver>(), get.Get<IWebBrowser>()));
            */

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
                    var dicts = Directory.EnumerateFiles(targetPath, "*.zip")
                        .Select(file => new YomichanTermDictionary(file, file + ".cache"));
                    foreach (var dict in dicts)
                    {
                        dictionaries.Add(dict);
                    }
                }
                catch (DirectoryNotFoundException)
                {

                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidOperationException(ex.Message + "\nIt was not possible to load the EPWING external dictionary.");
                }

                return dictionaries;
            }
        }

    }
}
