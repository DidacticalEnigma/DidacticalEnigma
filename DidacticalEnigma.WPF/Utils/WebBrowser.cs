using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DidacticalEnigma.Utils
{
    public interface IWebBrowser
    {
        void NavigateTo(Uri url);
    }

    public class WebBrowser : IWebBrowser
    {
        public void NavigateTo(Uri url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var u = url.ToString().Replace("&", "^&");
                using var p = Process.Start(new ProcessStartInfo("cmd", $"/c start {u}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                using var p = Process.Start("xdg-open", url.ToString());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                using var p = Process.Start("open", url.ToString());
            }
        }
    }
}