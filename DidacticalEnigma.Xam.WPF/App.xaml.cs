using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Xam.Models;
using DidacticalEnigma.Xam.Services;
using JDict;
using Xamarin.Forms;
using Application = System.Windows.Application;

namespace DidacticalEnigma.Xam.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            string dataDir = @"D:\DidacticalEnigma-Data";
            string cacheDir = @"C:\Users\IEUser\Desktop\cache";
            ServiceLocator.Configure(dataDir, cacheDir);
            /*ServiceLocator.Locator.BindFactory(get =>
            {
                return new KanjiRadicalLookupControlVM(new MockKanjiRadicalLookup(), new MockKanjiProperties(),
                    new MockRadicalSearcher(), new Dictionary<CodePoint, string>());
            });*/
        }
    }
}
