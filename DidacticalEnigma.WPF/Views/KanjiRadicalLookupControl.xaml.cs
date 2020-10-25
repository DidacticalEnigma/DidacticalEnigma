using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DidacticalEnigma.ViewModels;
using Utility.Utils;

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

        public ICommand ResetCommand
        {
            get { return (ICommand)GetValue(ResetCommandProperty); }
            set { SetValue(ResetCommandProperty, value); }
        }

        /// <summary>Identifies the <see cref="ResetCommand"/> dependency property.</summary>
        public static readonly DependencyProperty ResetCommandProperty =
            DependencyProperty.Register(nameof(ResetCommand), typeof(ICommand), typeof(KanjiRadicalLookupControl), new PropertyMetadata(null));

        public ICommand KeyClickCommand
        {
            get { return (ICommand)GetValue(KeyClickCommandProperty); }
            set { SetValue(KeyClickCommandProperty, value); }
        }

        /// <summary>Identifies the <see cref="KeyClickCommand"/> dependency property.</summary>
        public static readonly DependencyProperty KeyClickCommandProperty =
            DependencyProperty.Register(nameof(KeyClickCommand), typeof(ICommand), typeof(KanjiRadicalLookupControl), new PropertyMetadata(null));

        public IEnumerable<KanjiRadicalLookupControlVM.RadicalVM> Radicals
        {
            get { return (IEnumerable<KanjiRadicalLookupControlVM.RadicalVM>)GetValue(RadicalsProperty); }
            set { SetValue(RadicalsProperty, value); }
        }

        public static readonly DependencyProperty RadicalsProperty =
            DependencyProperty.Register(nameof(Radicals), typeof(IEnumerable<KanjiRadicalLookupControlVM.RadicalVM>), typeof(KanjiRadicalLookupControl), new PropertyMetadata(new PropertyChangedCallback(OnRadicalsChanged)));

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        /// <summary>Identifies the <see cref="SearchText"/> dependency property.</summary>
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(KanjiRadicalLookupControl), new PropertyMetadata(null));

        private List<KanjiRadicalLookupControlVM.RadicalVM> radicals = new List<KanjiRadicalLookupControlVM.RadicalVM>();

        private static void OnRadicalsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (KanjiRadicalLookupControl)d;
            foreach (var radical in self.radicals)
            {
                radical.PropertyChanged -= self.Callback;
            }
            self.radicals.Clear();
            var newValue = (IEnumerable<KanjiRadicalLookupControlVM.RadicalVM>)e.NewValue;
            foreach (var radical in newValue)
            {
                radical.PropertyChanged += self.Callback;
                self.radicals.Add(radical);
            }
        }

        private class RadicalVMComparer : IEqualityComparer<KanjiRadicalLookupControlVM.RadicalVM>
        {
            public bool Equals(KanjiRadicalLookupControlVM.RadicalVM x, KanjiRadicalLookupControlVM.RadicalVM y)
            {
                return x.CodePoint.Utf32 == y.CodePoint.Utf32;
            }

            public int GetHashCode(KanjiRadicalLookupControlVM.RadicalVM obj)
            {
                return obj.CodePoint.Utf32.GetHashCode();
            }
        }

        private HashSet<KanjiRadicalLookupControlVM.RadicalVM> selected = new HashSet<KanjiRadicalLookupControlVM.RadicalVM>(new RadicalVMComparer());

        private void Callback(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(KanjiRadicalLookupControlVM.RadicalVM.Selected))
            {
                var actual = (KanjiRadicalLookupControlVM.RadicalVM)sender;
                if (actual.Selected)
                {
                    selected.Add(actual);
                }
                else
                {
                    selected.Remove(actual);
                }
                (DataContext as KanjiRadicalLookupControlVM)?.SelectRadicals(selected.Select(r => r.CodePoint), Dispatcher);
            }
        }

        private void OnKeyPressedDown(object sender, KeyEventArgs e)
        {
            var bindingExpression = BindingOperations.GetBindingExpression((TextBox)sender, TextBox.TextProperty);
            if (e.Key == Key.Enter)
            {
                bindingExpression?.UpdateSource();
            }
            if (e.Key == Key.Escape)
            {
                bindingExpression?.UpdateTarget();
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (DataContext as KanjiRadicalLookupControlVM)?.SelectRadicals(selected.Select(r => r.CodePoint), Dispatcher);
        }
    }
}
