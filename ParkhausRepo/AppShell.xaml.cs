using ParkhausRepo.Views;

namespace ParkhausRepo
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(MapPage), typeof(MapPage));
        }
    }
}