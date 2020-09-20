using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.LanguageService;
using Gu.Inject;
using JDict;
using NMeCab;
using Optional.Collections;

namespace DidacticalEnigma.RestApi
{
    public class ServiceConfiguration
    {
        public const string ConfigurationName = "DataConfiguration";

        public string DataDirectory { get; set; }

        public static Kernel Configure(string dataDir)
        {
            var kernel = new Kernel();

            kernel.BindFactory(() => KanjiDict.Create(Path.Combine(dataDir, "character", "kanjidic2.xml.gz")));
            kernel.BindFactory(() => new Kradfile(Path.Combine(dataDir, "character", "kradfile1_plus_2_utf8"), Encoding.UTF8));
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
                DicDir = Path.Combine(dataDir, "mecab", "ipadic"),
                UseMemoryMappedFile = true
            }));
            kernel.BindFactory<IMorphologicalAnalyzer<UnidicEntry>>(() => new MeCabUnidic(new MeCabParam
            {
                DicDir = Path.Combine(dataDir, "mecab", "unidic"),
                UseMemoryMappedFile = true
            }));
            kernel.BindFactory<IMorphologicalAnalyzer<IEntry>>(get =>
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
            kernel.BindFactory(get => new RadicalRemapper(get.Get<Kradfile>(), get.Get<Radkfile>()));
            kernel.BindFactory(get => EasilyConfusedKana.FromFile(Path.Combine(dataDir, "character", "confused.txt")));
            kernel.Bind<IKanjiProperties, KanjiProperties>();
            kernel.BindFactory(get => new KanjiProperties(
                get.Get<KanjiDict>(),
                get.Get<Kradfile>(),
                get.Get<Radkfile>(),
                null));
            kernel.Bind<IRomaji, ModifiedHepburn>();
            kernel.BindFactory(get => new ModifiedHepburn(
                get.Get<IMorphologicalAnalyzer<IEntry>>(),
                get.Get<IKanaProperties>()));
            kernel.Bind<IAutoGlosser, AutoGlosserNext>();
            kernel.BindFactory(get => new AutoGlosserNext(get.Get<ISentenceParser>(), get.Get<JMDictLookup>(), get.Get<IKanaProperties>()));
            kernel.Bind<IKanaProperties, KanaProperties2>();
            kernel.BindFactory(get => new KanaProperties2(Path.Combine(dataDir, "character", "kana.txt"), Encoding.UTF8));
            kernel.BindFactory(get => new SimilarKanji(Path.Combine(dataDir, "character", "kanji.tgz_similars.ut8"), Encoding.UTF8));
            kernel.BindFactory(get => new SentenceParser(get.Get<IMorphologicalAnalyzer<IEntry>>(), get.Get<JMDictLookup>()));
            kernel.Bind<ISentenceParser, SentenceParser>();
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
                using (var reader = System.IO.File.OpenText(Path.Combine(dataDir, "character", "radkfile1_plus_2_utf8")))
                    return new KanjiRadicalLookup(Radkfile.Parse(reader), get.Get<KanjiDict>());
            });
            kernel.BindFactory<IJGramLookup>(get => new JGramLookup(
                Path.Combine(dataDir, "dictionaries", "jgram"),
                Path.Combine(dataDir, "dictionaries", "jgram_lookup"),
                Path.Combine(dataDir, "dictionaries", "jgram.cache")));
            kernel.BindFactory(get => new RadkfileKanjiAliveCorrelator(Path.Combine(dataDir, "character", "radkfile_kanjilive_correlation_data.txt")));
            kernel.Bind<IRadicalSearcher, RadicalSearcher>();
            kernel.BindFactory(get => new RadicalSearcher(
                get.Get<KanjiRadicalLookup>().AllRadicals,
                KanjiAliveJapaneseRadicalInformation.Parse(Path.Combine(dataDir, "character", "japanese-radicals.csv")),
                get.Get<RadkfileKanjiAliveCorrelator>()));
            kernel.BindFactory(get => new Corpus(get.Get<Tanaka>().AllSentences, get.Get<IMorphologicalAnalyzer<IpadicEntry>>(), Path.Combine(dataDir, "corpora", "tanaka.cache")));

            kernel.BindFactory(get => new KeyValuePair<string, IDataSource>[] {
                new KeyValuePair<string, IDataSource>(null, new CharacterDataSource(get.Get<IKanjiProperties>(), get.Get<IKanaProperties>())),
                new KeyValuePair<string, IDataSource>(null, new CharacterStrokeOrderDataSource()),
                new KeyValuePair<string, IDataSource>(null, new JMDictDataSource(get.Get<JMDictLookup>(), get.Get<IKanaProperties>())),
                new KeyValuePair<string, IDataSource>(null, new JNeDictDataSource(get.Get<Jnedict>())),
                new KeyValuePair<string, IDataSource>(null, new VerbConjugationDataSource(get.Get<JMDictLookup>())),
                new KeyValuePair<string, IDataSource>(null, new WordFrequencyRatingDataSource(get.Get<FrequencyList>())),
                new KeyValuePair<string, IDataSource>(null, new PartialExpressionJMDictDataSource(get.Get<IdiomDetector>())),
                new KeyValuePair<string, IDataSource>(null, new JGramDataSource(get.Get<IJGramLookup>())),
                new KeyValuePair<string, IDataSource>(null, new AutoGlosserDataSource(get.Get<IAutoGlosser>())),
                new KeyValuePair<string, IDataSource>(null, new CustomNotesDataSource(Path.Combine(dataDir, "custom", "custom_notes.txt"))),
                new KeyValuePair<string, IDataSource>(null, new TanakaCorpusFastDataSource(get.Get<Corpus>())),
                new KeyValuePair<string, IDataSource>(null, new TanakaCorpusDataSource(get.Get<Tanaka>())),
                new KeyValuePair<string, IDataSource>(null, new BasicExpressionCorpusDataSource(get.Get<BasicExpressionsCorpus>())),
                new KeyValuePair<string, IDataSource>(null, new PartialWordLookupJMDictDataSource(get.Get<PartialWordLookup>(), get.Get<FrequencyList>())),
                new KeyValuePair<string, IDataSource>(null, new JESCDataSource(get.Get<JESC>())),
                new KeyValuePair<string, IDataSource>(null, new RomajiDataSource(get.Get<IRomaji>()))
            }.Concat(get.Get<EpwingDictionaries>().Dictionaries.Select(dict => new KeyValuePair<string, IDataSource>(dict.Revision, new EpwingDataSource(dict, get.Get<IKanaProperties>())))));


            return kernel;

            IReadOnlyDictionary<CodePoint, string> CreateTextRadicalMappings(IEnumerable<CodePoint> radicals, IReadOnlyDictionary<int, int> remapper)
            {
                var dict = radicals.ToDictionary(
                    r => r,
                    r => Char.ConvertFromUtf32(remapper.GetValueOrNone(r.Utf32).ValueOr(r.Utf32)));
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
                catch (InvalidOperationException)
                {

                }

                return dictionaries;
            }
        }
    }
}