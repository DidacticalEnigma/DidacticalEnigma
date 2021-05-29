using DidacticalEnigma.Updater.WPF.ViewModels;
using DidacticalEnigma.Updater.WPF.Views;
using System;
using System.IO;
using System.Net.Http;
using System.Windows;

namespace DidacticalEnigma.Updater.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Startup += async (sender, args) =>
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dataDir = Path.Combine(baseDir, "Data");
                var window = new MainWindow();
                var httpClient = new HttpClient();
                window.DataContext = new MainWindowVM(new UpdaterProcess[]
                {
                    new JMDictUpdaterProcess(
                        httpClient,
                        "http://ftp.edrdg.org/pub/Nihongo/JMdict_e.gz",
                        Path.Combine(dataDir, "dictionaries", "JMdict_e.gz"),
                        Path.Combine(dataDir, "dictionaries", "JMdict_e.cache"),
                        Path.Combine(dataDir, "dictionaries", "JMdict_e.gz.new"),
                        Path.Combine(dataDir, "dictionaries", "JMdict_e.new.cache"),
                        Path.Combine(dataDir, "dictionaries", "jmdict_tested_schema.xml")),
                    new JMNedictUpdaterProcess(
                        httpClient,
                        "http://ftp.edrdg.org/pub/Nihongo/JMnedict.xml.gz",
                        Path.Combine(dataDir, "dictionaries", "JMnedict.xml.gz"),
                        Path.Combine(dataDir, "dictionaries", "JMnedict.xml.cache"),
                        Path.Combine(dataDir, "dictionaries", "JMnedict.xml.gz.new"),
                        Path.Combine(dataDir, "dictionaries", "JMnedict.xml.new.cache"),
                        Path.Combine(dataDir, "dictionaries", "jmnedict_tested_schema.xml")),
                    new KanjiDictUpdaterProcess(
                        httpClient,
                        "http://ftp.edrdg.org/pub/Nihongo/kanjidic2.xml.gz",
                        Path.Combine(dataDir, "character", "kanjidic2.xml.gz"),
                        Path.Combine(dataDir, "character", "kanjidic2.xml.gz.new"))
                });
                window.Show();
            };
            Exit += (sender, args) =>
            {

            };
        }
    }
}
