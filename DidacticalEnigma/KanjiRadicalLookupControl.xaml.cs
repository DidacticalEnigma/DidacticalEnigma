using DidacticalEnigma.Models;
using DidacticalEnigma.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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
using static DidacticalEnigma.ViewModels.KanjiRadicalLookupControlVM;

namespace DidacticalEnigma
{
    /// <summary>
    /// Interaction logic for KanjiRadicalLookupControl.xaml
    /// </summary>
    public partial class KanjiRadicalLookupControl : UserControl
    {
        public KanjiRadicalLookupControl()
        {
            InitializeComponent();
        }

        private void ListView_Selected(object sender, RoutedEventArgs e)
        {
            var vm = (KanjiRadicalLookupControlVM)DataContext;
            vm.SelectRadicals(RadicalSelector.SelectedItems.Cast<RadicalVM>().Select(r => r.CodePoint));
        }

        public ICommand KeyClickCommand
        {
            get { return (ICommand)GetValue(KeyClickCommandProperty); }
            set { SetValue(KeyClickCommandProperty, value); }
        }

        /// <summary>Identifies the <see cref="KeyClickCommand"/> dependency property.</summary>
        public static readonly DependencyProperty KeyClickCommandProperty =
            DependencyProperty.Register("KeyClickCommand", typeof(ICommand), typeof(KanjiRadicalLookupControl), new PropertyMetadata(null));
    }
}
