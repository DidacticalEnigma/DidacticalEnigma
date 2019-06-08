using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Logging.Serilog;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.ViewModels;
using Utility.Utils;

namespace DidacticalEnigma.Avalonia
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) => BuildAvaloniaApp().Start(AppMain, args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug();

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void AppMain(Application app, string[] args)
        {
            
            app.Run(new MainWindow()
            {
                
            });
        }
    }

    public static class Mocks
    {
        public class SentenceParser : ISentenceParser
        {
            public IEnumerable<IEnumerable<WordInfo>> BreakIntoSentences(string input)
            {
                return EnumerableExt.OfSingle(input.Split(' ').Select(w => new WordInfo(w)));
            }
        }
    }
}
