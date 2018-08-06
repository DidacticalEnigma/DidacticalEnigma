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

namespace DidacticalEnigma
{
    /// <summary>
    /// Interaction logic for UsageDataSourcePreview.xaml
    /// </summary>
    public partial class UsageDataSourcePreview : UserControl
    {
        // Using a DependencyProperty as the backing store for Lines.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchQueryProperty =
            DependencyProperty.Register(
                nameof(SearchQuery),
                typeof(Request),
                typeof(UsageDataSourcePreview),
                new PropertyMetadata(null, OnSearchQueryChanged));

        private static void OnSearchQueryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (UsageDataSourcePreview)d;
            var oldValue = (Request)e.OldValue;
            var newValue = (Request)e.NewValue;

            if(self.DataContext is UsageDataSourcePreviewVM vm)
            {
                vm.Request = newValue;
            }
        }

        public Request SearchQuery
        {
            get { return (Request)GetValue(SearchQueryProperty); }
            set { SetValue(SearchQueryProperty, value); }
        }

        public UsageDataSourcePreview()
        {
            InitializeComponent();
        }
    }
}
