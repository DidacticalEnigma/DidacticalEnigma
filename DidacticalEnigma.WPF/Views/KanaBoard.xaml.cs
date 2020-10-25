using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DidacticalEnigma.Views
{
    /// <summary>
    /// Interaction logic for KanaBoard.xaml
    /// </summary>
    public partial class KanaBoard : UserControl
    {
        public KanaBoard()
        {
            InitializeComponent();
        }

        public ICommand KeyClickCommand
        {
            get { return (ICommand)GetValue(KeyClickCommandProperty); }
            set { SetValue(KeyClickCommandProperty, value); }
        }

        /// <summary>Identifies the <see cref="KeyClickCommand"/> dependency property.</summary>
        public static readonly DependencyProperty KeyClickCommandProperty =
            DependencyProperty.Register(nameof(KeyClickCommand), typeof(ICommand), typeof(KanaBoard), new PropertyMetadata(null));


    }
}
