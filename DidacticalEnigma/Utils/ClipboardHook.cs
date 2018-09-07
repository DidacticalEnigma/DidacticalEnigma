using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace DidacticalEnigma.Utils
{
    // Not actually a clipboard hook
    // This does polling every now and then instead
    // TODO: make a proper implementation
    class ClipboardHook : IDisposable
    {
        private string previous = "";

        private DispatcherTimer timer;

        public event EventHandler<string> ClipboardChanged;

        public ClipboardHook()
        {
            timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, TimerCallback, Dispatcher.CurrentDispatcher);
            timer.Start();
        }

        private void TimerCallback(object sender, EventArgs e)
        {
            try
            {
                RaiseClipboardChanged(Clipboard.GetText());
            }
            catch (COMException)
            {
                // trying to silence "data in clipboard is invalid" exception
                // we simply won't update our clipboard in that case
            }
        }

        public void Dispose()
        {
            timer.Stop();
        }

        protected void RaiseClipboardChanged(string newData)
        {
            if (newData == "")
                return;
            if (previous == newData)
                return;
            previous = newData;
            ClipboardChanged?.Invoke(this, newData);
        }
    }
}
