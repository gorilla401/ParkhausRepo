using Microsoft.Extensions.Logging;
using ParkhausRepo.Controllers;
using ParkhausRepo.Services;
using ParkhausRepo.Views;

namespace ParkhausRepo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<CarSpawningService>();

            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<MapViewModel>();

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MapPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}