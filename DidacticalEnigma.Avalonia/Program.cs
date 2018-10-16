using System;
using Avalonia;
using Avalonia.Logging.Serilog;

namespace DidacticalEnigma.Avalonia
{
    class Program
    {
        static void Main(string[] args)
        {
            BuildAvaloniaApp().Start<MainWindow>();
        }

        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug();
    }
}
