using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DidacticalEnigma.Utils
{
    // based on
    // https://codereview.stackexchange.com/q/115417/25793
    internal sealed class ClipboardHook : IDisposable
    {
        private static class NativeMethods
        {
            /// <summary>
            /// Places the given window in the system-maintained clipboard format listener list.
            /// </summary>
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AddClipboardFormatListener(IntPtr hwnd);

            /// <summary>
            /// Removes the given window from the system-maintained clipboard format listener list.
            /// </summary>
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

            /// <summary>
            /// Sent when the contents of the clipboard have changed.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            public const int WM_CLIPBOARDUPDATE = 0x031D;

            /// <summary>
            /// To find message-only windows, specify HWND_MESSAGE in the hwndParent parameter of the FindWindowEx function.
            /// </summary>
            public static IntPtr HWND_MESSAGE = new IntPtr(-3);
        }

        private bool disposed = false;

        private HwndSource hwndSource = new HwndSource(0, 0, 0, 0, 0, 0, 0, null, NativeMethods.HWND_MESSAGE);

        public ClipboardHook()
        {
            hwndSource.AddHook(WndProc);
            NativeMethods.AddClipboardFormatListener(hwndSource.Handle);
        }

        public void SetText(string s)
        {
            Clipboard.SetText(s);
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            NativeMethods.RemoveClipboardFormatListener(hwndSource.Handle);
            hwndSource.RemoveHook(WndProc);
            hwndSource.Dispose();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_CLIPBOARDUPDATE)
            {
                try
                {
                    var text = Clipboard.GetText();
                    ClipboardChanged?.Invoke(this, text);
                }
                catch (COMException)
                {
                    // trying to silence "data in clipboard is invalid" exception
                    // we simply won't update our clipboard in that case
                }
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Occurs when the clipboard content changes.
        /// </summary>
        public event EventHandler<string> ClipboardChanged;
    }
}
