using DidacticalEnigma.Mem.Client;
using DidacticalEnigma.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DidacticalEnigma.Views
{
    /// <summary>
    /// Interaction logic for DidacticalEnigmaMemSettings.xaml
    /// </summary>
    public partial class DidacticalEnigmaMemSettings : UserControl
    {
        private DidacticalEnigmaMemViewModel fixture;

        public DidacticalEnigmaMemViewModel Fixture
        {
            get { return (DidacticalEnigmaMemViewModel)GetValue(FixtureProperty); }
            set { SetValue(FixtureProperty, value); }
        }

        /// <summary>Identifies the <see cref="Lines"/> dependency property.</summary>
        public static readonly DependencyProperty FixtureProperty =
            DependencyProperty.Register(
                nameof(Fixture),
                typeof(DidacticalEnigmaMemViewModel),
                typeof(DidacticalEnigmaMemSettings),
                new PropertyMetadata(null, OnFixtureChanged));

        private static void OnFixtureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (DidacticalEnigmaMemSettings)d;
            var newValue = (DidacticalEnigmaMemViewModel)e.NewValue;
            self.fixture = newValue;
        }

        private IWebBrowser webBrowser;

        public IWebBrowser WebBrowser
        {
            get { return (IWebBrowser)GetValue(WebBrowserProperty); }
            set { SetValue(WebBrowserProperty, value); }
        }

        /// <summary>Identifies the <see cref="Lines"/> dependency property.</summary>
        public static readonly DependencyProperty WebBrowserProperty =
            DependencyProperty.Register(
                nameof(WebBrowser),
                typeof(IWebBrowser),
                typeof(DidacticalEnigmaMemSettings),
                new PropertyMetadata(null, OnWebBrowserChanged));

        private static void OnWebBrowserChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (DidacticalEnigmaMemSettings)d;
            var newValue = (IWebBrowser)e.NewValue;
            self.webBrowser = newValue;
        }

        public DidacticalEnigmaMemSettings()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            webBrowser?.NavigateTo(e.Uri);
        }
    }
}
