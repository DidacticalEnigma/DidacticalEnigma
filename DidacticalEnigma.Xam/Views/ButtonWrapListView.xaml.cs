using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Xam.Services;
using Utility.Utils;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DidacticalEnigma.Xam.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ButtonWrapListView : ContentView
    {
        public ButtonWrapListView()
        {

            InitializeComponent();
            var sentinel = new RadicalVM("", CodePoint.FromInt(0), false);
            var n = 10;
            M.ItemsSource = Enumerable.Range(0, 2003)
                .Select(x => new RadicalVM(((char)x).ToString(), CodePoint.FromInt(0)))
                .ChunkBy(n)
                .Select(chunk => chunk
                    .Concat(EnumerableExt.Repeat(sentinel))
                    .Take(n)
                    .ToList()).ToList();
        }
    }

    public class RadicalVM
    {
        public bool IsEnabled { get; }

        public string Text { get; }

        public CodePoint Radical { get; }

        public RadicalVM(string text, CodePoint radical, bool isEnabled = true)
        {
            Text = text;
            Radical = radical;
            IsEnabled = isEnabled;
        }
    }
}