using Avalonia;
using Avalonia.Markup.Xaml;

namespace DidacticalEnigma.Avalonia
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
