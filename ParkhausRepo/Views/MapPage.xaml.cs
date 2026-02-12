using Microsoft.Maui.Controls;
using ParkhausRepo.Controllers;

namespace ParkhausRepo.Views
{
    public partial class MapPage : ContentPage
    {
        private readonly MapViewModel _viewModel;

        public MapPage(MapViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadCommand.Execute(null);
        }
    }
}
