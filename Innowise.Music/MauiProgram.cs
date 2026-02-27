using Innowise.Music.Services;
using Innowise.Music.View;
using Innowise.Music.ViewModel;
using Microsoft.Extensions.Logging;
//using Microsoft.AspNetCore.Components.Authorization;

namespace Innowise.Music
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
                    fonts.AddFont("Lexend-Regular.ttf", "LexendRegular");
                    fonts.AddFont("Lexend-Semibold.ttf", "LexendSemibold");
                    fonts.AddFont("Lexend-ExtraBold.ttf", "LexendExtrabold");
                });
            builder.Services.AddSingleton<HttpHelper>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<WebNewsService>();
            builder.Services.AddSingleton<MockNewsService>();
            builder.Services.AddSingleton<NewsPageViewModel>();
            builder.Services.AddSingleton<NewsPage>();
            builder.Services.AddSingleton<NewsDetailedPageViewModel>();
            builder.Services.AddSingleton<NewsDetailedPage>();
            builder.Services.AddSingleton<LoginPageViewModel>();
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<SignUpPageViewModel>();
            builder.Services.AddSingleton<SignUpPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
