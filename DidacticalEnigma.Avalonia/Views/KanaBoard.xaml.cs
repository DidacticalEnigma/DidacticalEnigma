using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DidacticalEnigma.Avalonia.Views
{
    public class KanaBoard : UserControl
    {
        public KanaBoard()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public ObservableCollection<string> Vms { get; } = new ObservableCollection<string>{ "aa", "bbb", "cc", "ddd", "ee", "ff" };
    }
}
