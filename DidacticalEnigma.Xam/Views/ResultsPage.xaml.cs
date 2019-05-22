using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using DidacticalEnigma.Xam.Models;
using DidacticalEnigma.Xam.Views;
using DidacticalEnigma.Xam.ViewModels;

namespace DidacticalEnigma.Xam.Views
{
    public partial class ResultsPage : ContentPage
    {
        ItemsViewModel viewModel;

        public ResultsPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new ItemsViewModel();
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var item = args.SelectedItem as Item;
            if (item == null)
                return;

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Items.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);
        }
    }
}