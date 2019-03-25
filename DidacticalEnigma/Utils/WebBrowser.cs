using System;
using System.Diagnostics;

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
            using (Process.Start(url.ToString()))
            {

            }
        }
    }
}