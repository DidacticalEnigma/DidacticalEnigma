using System.Windows;
using System.Windows.Controls;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Models;
using DidacticalEnigma.ViewModels;

namespace DidacticalEnigma.Views
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
