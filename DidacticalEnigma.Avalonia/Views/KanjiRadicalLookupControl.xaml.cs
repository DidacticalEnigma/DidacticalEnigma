using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DidacticalEnigma.Avalonia.Views
{
    public class KanjiRadicalLookupControl : UserControl
    {
        public KanjiRadicalLookupControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
