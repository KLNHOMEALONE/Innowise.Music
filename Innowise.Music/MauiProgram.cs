using CommunityToolkit.Maui;
using Innowise.Music.Configuration;
using Innowise.Music.Controls;
using Innowise.Music.Services;
using Innowise.Music.View;
using Innowise.Music.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Innowise.Music
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            
            var assembly = Assembly.GetExecutingAssembly();
            
            // Load main appsettings.json
            var stream = assembly.GetManifestResourceStream("Innowise.Music.appsettings.json");
            if (stream != null)
            {
                builder.Configuration.AddJsonStream(stream);
            }

            // Load local overrides if they exist
            var localStream = assembly.GetManifestResourceStream("Innowise.Music.appsettings.local.json");
            if (localStream != null)
            {
                builder.Configuration.AddJsonStream(localStream);
            }
            
            builder
                .UseMauiApp<App>()
#if !WINDOWS
                .UseMauiMaps()
#endif
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Lexend-Regular.ttf", "LexendRegular");
                    fonts.AddFont("Lexend-Semibold.ttf", "LexendSemibold");
                    fonts.AddFont("Lexend-ExtraBold.ttf", "LexendExtrabold");
                });
            
            // Register configuration
            builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.SectionName));
            builder.Services.Configure<GoogleAuthenticationSettings>(builder.Configuration.GetSection(GoogleAuthenticationSettings.SectionName));
            builder.Services.AddSingleton<IHttpHelper, HttpHelper>();
            builder.Services.AddSingleton<HttpClient>(provider =>
            {
                var httpHelper = provider.GetRequiredService<IHttpHelper>();
                return new HttpClient(httpHelper.GetInsecureHandler());
            });

            builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
            builder.Services.AddSingleton<IGoogleAuthService, GoogleAuthService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton<IHealthCheckService, HealthCheckService>();
            builder.Services.AddSingleton<IAudioService, AudioService>();
            builder.Services.AddSingleton<ISearchService, SearchService>();
            builder.Services.AddSingleton<IStreamTokenService, StreamTokenService>();
            builder.Services.AddSingleton<IRecommendationService, RecommendationService>();
            builder.Services.AddSingleton<IHistoryService, HistoryService>();
            builder.Services.AddSingleton<IFavoriteService, FavoriteService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            
            // Transient ViewModels and Pages (Resets state on each navigation)
            builder.Services.AddTransient<LoginPageViewModel>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SignUpPageViewModel>();
            builder.Services.AddTransient<SignUpPage>();
            builder.Services.AddTransient<WebPage>();
            builder.Services.AddTransient<WebPageViewModel>();

            // Singleton ViewModels and Pages (Maintains state/caches data)
            builder.Services.AddSingleton<HomePageViewModel>();
            builder.Services.AddSingleton<HomePage>();
            builder.Services.AddSingleton<SearchPageViewModel>();
            builder.Services.AddSingleton<SearchPage>();
            builder.Services.AddSingleton<LibraryPageViewModel>();
            builder.Services.AddSingleton<LibraryPage>();
            builder.Services.AddSingleton<EventsPageViewModel>();
            builder.Services.AddSingleton<EventsPage>();
            builder.Services.AddSingleton<MiniPlayerViewModel>();
            builder.Services.AddTransient<MiniPlayerControl>();
            builder.Services.AddSingleton<AppShellViewModel>();
            builder.Services.AddSingleton<AppShell>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
