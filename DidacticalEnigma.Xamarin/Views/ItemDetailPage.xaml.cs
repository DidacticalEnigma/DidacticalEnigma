using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using DidacticalEnigma.Xamarin.Models;
using DidacticalEnigma.Xamarin.ViewModels;

namespace DidacticalEnigma.Xamarin.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        ItemDetailViewModel viewModel;

        public ItemDetailPage(ItemDetailViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = this.viewModel = viewModel;
        }

        public ItemDetailPage()
        {
            InitializeComponent();

            var item = new Item
            {
                Text = "Item 1",
                Description = "This is an item description."
            };

            viewModel = new ItemDetailViewModel(item);
            BindingContext = viewModel;
        }
    }
}