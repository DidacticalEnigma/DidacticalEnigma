using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Xam.Services;
using Utility.Utils;
using Xamarin.Forms;

namespace DidacticalEnigma.Xam.ViewModels
{
    public class InputViewModel : BaseViewModel
    {
        public InputViewModel()
        {
            Title = "Input";
            ParseTextCommand = new Command(() =>
            {

            });
        }

        public static readonly IEnumerable<int> Lol = new ObservableBatchCollection<int>(Enumerable.Range(0, 30));

        public ICommand ParseTextCommand { get; }

        private string inputText = "";
        public string InputText
        {
            get { return inputText; }
            set { SetProperty(ref inputText, value); }
        }

        public KanjiRadicalLookupControlVM RadicalLookup { get; } =
            ServiceLocator.Locator.Get<KanjiRadicalLookupControlVM>();
    }
}