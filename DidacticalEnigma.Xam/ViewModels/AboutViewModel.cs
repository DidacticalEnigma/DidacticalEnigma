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

            
        }

        public string AboutText { get; }
    }
}