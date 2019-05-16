using System;
using System.Windows.Input;

using Xamarin.Forms;

namespace DidacticalEnigma.Xam.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";

            AboutText = "This application uses the following data sources:\n\n\n- Here\n- we\n-go\n";
        }

        public string AboutText { get; }
    }
}