using System;
using System.Diagnostics;
using System.Threading;
using DidacticalEnigma;
using DidacticalEnigma.ViewModels;
using NUnit.Framework;

namespace JDict.Tests
{
    [RequiresThread(ApartmentState.STA)]
    public class ProgramStartupPerformanceTests
    {
        [Explicit]
        [Test]
        public void StartupTimeTest()
        {
            // cold boot
            using (var kernel = App.Configure(TestDataPaths.BaseDir))
            {
                kernel.BindFactory<ITextInsertCommand>(() => new MockInsertTextCommand());
                _ = kernel.Get<MainWindowVM>();
            }

            var watch = Stopwatch.StartNew();
            // hot boot
            using (var kernel = App.Configure(TestDataPaths.BaseDir))
            {
                kernel.BindFactory<ITextInsertCommand>(() => new MockInsertTextCommand());
                _ = kernel.Get<MainWindowVM>();
                watch.Stop();
            }

            Assert.Less(watch.Elapsed, TimeSpan.FromSeconds(10));
        }

        private class MockInsertTextCommand : ITextInsertCommand
        {
            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {

            }

            public event EventHandler CanExecuteChanged;
        }
    }
}
