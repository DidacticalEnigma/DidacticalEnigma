using DidacticalEnigma.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DidacticalEnigma.Views
{
    /// <summary>
    /// Interaction logic for ReplControl.xaml
    /// </summary>
    public partial class ReplControl : UserControl
    {
        public ReplControl()
        {
            InitializeComponent();
            Loaded += LoadedHandler;
        }

        public ReplVM Repl
        {
            get { return (ReplVM)GetValue(ReplProperty); }
            set { SetValue(ReplProperty, value); }
        }

        /// <summary>Identifies the <see cref="Repl"/> dependency property.</summary>
        public static readonly DependencyProperty ReplProperty =
            DependencyProperty.Register(
                nameof(Repl),
                typeof(ReplVM),
                typeof(ReplControl),
                new PropertyMetadata(null));

        void LoadedHandler(object sender, RoutedEventArgs e)
        {
            InputBlock.KeyUp += KeyDownHandler;
            InputBlock.Focus();
        }

        void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Repl.ConsoleInput = InputBlock.Text;
                if(InputBlock.Text == "")
                {
                    return;
                }

                Repl.RunCommand();
                InputBlock.Focus();
                Scroller.ScrollToBottom();
            }
        }
    }
}
