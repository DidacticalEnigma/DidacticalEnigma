using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DidacticalEnigma.ViewModels;

namespace DidacticalEnigma.Views
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
            vm.SelectRadicals(RadicalSelector.SelectedItems.Cast<KanjiRadicalLookupControlVM.RadicalVM>().Select(r => r.CodePoint), Dispatcher);
        }

        public ICommand KeyClickCommand
        {
            get { return (ICommand)GetValue(KeyClickCommandProperty); }
            set { SetValue(KeyClickCommandProperty, value); }
        }

        /// <summary>Identifies the <see cref="KeyClickCommand"/> dependency property.</summary>
        public static readonly DependencyProperty KeyClickCommandProperty =
            DependencyProperty.Register(nameof(KeyClickCommand), typeof(ICommand), typeof(KanjiRadicalLookupControl), new PropertyMetadata(null));

        private void ResetOnClick(object sender, RoutedEventArgs e)
        {
            var vm = (KanjiRadicalLookupControlVM)DataContext;
            RadicalSelector.SelectedItems.Clear();
            vm.SelectRadicals(Enumerable.Empty<KanjiRadicalLookupControlVM.RadicalVM>().Select(r => r.CodePoint), Dispatcher);
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = (KanjiRadicalLookupControlVM)DataContext;
            vm.SelectRadicals(RadicalSelector.SelectedItems.Cast<KanjiRadicalLookupControlVM.RadicalVM>().Select(r => r.CodePoint), Dispatcher);
        }
    }
}
