using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DidacticalEnigma.Avalonia.Views
{
    public class UsageDataSourcePreview : UserControl
    {
        public UsageDataSourcePreview()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
