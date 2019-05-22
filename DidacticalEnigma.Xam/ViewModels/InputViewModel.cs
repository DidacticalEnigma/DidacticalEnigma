using System.Collections.ObjectModel;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Xam.Services;
using Xamarin.Forms;

namespace DidacticalEnigma.Xam.ViewModels
{
    public class InputViewModel : BaseViewModel
    {
        public InputViewModel()
        {
            Title = "Input";
        }

        public string InputText { get; set; }

        public KanjiRadicalLookupControlVM RadicalLookup { get; } =
            ServiceLocator.Locator.Get<KanjiRadicalLookupControlVM>();
    }
}