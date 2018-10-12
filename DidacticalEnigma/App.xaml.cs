using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DidacticalEnigma.Core.Models;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Models;
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
            };
        }

        private void Configure()
        {
            var kernel = new Kernel();
            // var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var baseDir = @"D:\DidacticalEnigma-Data";
            kernel.BindFactory(() => JDict.KanjiDict.Create(Path.Combine(baseDir, "character", "kanjidic2.xml.gz")));
            kernel.BindFactory(() => new Kradfile(Path.Combine(baseDir, "character", "kradfile1_plus_2_utf8"), Encoding.UTF8));
            kernel.BindFactory(() => new Radkfile(Path.Combine(baseDir, "character", "radkfile1_plus_2_utf8"), Encoding.UTF8));
            kernel.BindFactory(() => JDict.JMDict.Create(Path.Combine(baseDir, "dictionaries", "JMdict_e.gz"), Path.Combine(baseDir, "dictionaries", "JMdict_e.cache")));
            kernel.BindFactory(() => JDict.Jnedict.Create(Path.Combine(baseDir, "dictionaries", "JMnedict.xml.gz"), Path.Combine(baseDir, "dictionaries", "JMnedict.xml.cache")));
            kernel.BindFactory(() =>
                new FrequencyList(Path.Combine(baseDir, "other", "word_form_frequency_list.txt"), Encoding.UTF8));
            kernel.BindFactory(() => new KanaProperties(
                katakanaPath: Path.Combine(baseDir, "character", "hiragana_romaji.txt"),
                hiraganaPath: Path.Combine(baseDir, "character", "katakana_romaji.txt"),
                hiraganaKatakanaPath: Path.Combine(baseDir, "character", "hiragana_katakana.txt"),
                complexPath: Path.Combine(baseDir, "character", "kana_related.txt"),
                encoding: Encoding.UTF8));
            kernel.BindFactory(() => new Tanaka(Path.Combine(baseDir, "corpora", "examples.utf.gz"), Encoding.UTF8));
            kernel.BindFactory(() => new JESC(Path.Combine(baseDir, "corpora", "jesc_raw"), Encoding.UTF8));
            kernel.BindFactory(() => new BasicExpressionsCorpus(Path.Combine(baseDir, "corpora", "JEC_basic_sentence_v1-2.csv"), Encoding.UTF8));
            kernel.BindFactory<ILanguageService>(get => new LanguageService(
                new MeCab(new MeCabParam
                {
                    DicDir = Path.Combine(baseDir, "mecab", "ipadic"),
                }),
                EasilyConfusedKana.FromFile(Path.Combine(baseDir, "character", "confused.txt")),
                get.Get<Kradfile>(),
                get.Get<Radkfile>(),
                get.Get<KanjiDict>(),
                get.Get<KanaProperties>()));
            kernel.BindFactory(get => new MainWindowVM(
                get.Get<ILanguageService>(),
                get.Get<JMDict>(),
                get.Get<FrequencyList>(),
                get.Get<Jnedict>(),
                new KanaBoardVM(Path.Combine(baseDir, "character", "hiragana_romaji.txt"), Encoding.UTF8, get.Get<ILanguageService>()),
                new KanaBoardVM(Path.Combine(baseDir, "character", "katakana_romaji.txt"), Encoding.UTF8, get.Get<ILanguageService>()),
                get.Get<Tanaka>(),
                get.Get<JESC>(),
                get.Get<BasicExpressionsCorpus>(),
                Path.Combine(baseDir, "custom", "custom_notes.txt")));
            Kernel = kernel;
        }
    }
}
