using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Models;
using DidacticalEnigma.ViewModels;
using DidacticalEnigma.Views;
using Gu.Inject;
using JDict;
using NMeCab;

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
            Configure();
            Startup += (sender, args) =>
            {
                var vm = Kernel.Get<MainWindowVM>();
                var window = new MainWindow()
                {
                    DataContext = vm
                };
                window.Show();
            };
            Exit += (sender, args) =>
            {
                Kernel.Dispose();
            };
        }

        private void Configure()
        {
            var kernel = new Kernel();
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            kernel.BindFactory(() => JDict.KanjiDict.Create(Path.Combine(baseDir, @"dic\kanjidic2.xml")));
            kernel.BindFactory(() => new Kradfile(Path.Combine(baseDir, @"dic\kradfile1_plus_2_utf8"), Encoding.UTF8));
            kernel.BindFactory(() => new Radkfile(Path.Combine(baseDir, @"dic\radkfile1_plus_2_utf8"), Encoding.UTF8));
            kernel.BindFactory(() => JDict.JMDict.Create(Path.Combine(baseDir, "dic", "JMdict_e"), Path.Combine(baseDir, "dic", "JMdict_e.cache")));
            kernel.BindFactory(() =>
                new FrequencyList(Path.Combine(baseDir, @"dic\word_form_frequency_list.txt"), Encoding.UTF8));
            kernel.BindFactory(() => new KanaProperties(
                katakanaPath: Path.Combine(baseDir, @"dic\hiragana_romaji.txt"),
                hiraganaPath: Path.Combine(baseDir, @"dic\katakana_romaji.txt"),
                hiraganaKatakanaPath: Path.Combine(baseDir, @"dic\hiragana_katakana.txt"),
                complexPath: Path.Combine(baseDir, @"dic\kana_related.txt"),
                encoding: Encoding.UTF8));
            kernel.BindFactory<ILanguageService>(get => new LanguageService(
                new MeCab(new MeCabParam
                {
                    DicDir = Path.Combine(baseDir, @"dic\ipadic"),
                }),
                EasilyConfusedKana.FromFile(Path.Combine(baseDir, @"dic\confused.txt")),
                get.Get<Kradfile>(),
                get.Get<Radkfile>(),
                get.Get<KanjiDict>(),
                get.Get<KanaProperties>()));
            kernel.BindFactory(get => new MainWindowVM(
                get.Get<ILanguageService>(),
                get.Get<JMDict>(),
                get.Get<FrequencyList>(),
                baseDir));
            Kernel = kernel;
        }
    }
}
