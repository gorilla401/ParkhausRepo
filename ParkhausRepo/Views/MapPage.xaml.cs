using Microsoft.Maui.Controls;
using ParkhausRepo.Controllers;

namespace ParkhausRepo.Views
{
    public partial class MapPage : ContentPage
    {
        public MapPage(MapViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
