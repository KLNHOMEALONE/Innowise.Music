using Innowise.Music.Services;

namespace Innowise.Music
{
    public partial class App : Application
    {
        private readonly Services.IAuthService _authService;
        private readonly INavigationService _navigationService;

        public App(Services.IAuthService authService, INavigationService navigationService)
        {
            InitializeComponent();
            _authService = authService;
            _navigationService = navigationService;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());
            CheckAuthStatus();
            return window;
        }

        private async void CheckAuthStatus()
        {
            if (await _authService.IsAuthenticatedAsync())
            {
                await _navigationService.NavigateToAsync($"///{nameof(View.NewsPage)}");
            }
            else
            {
                await _navigationService.NavigateToAsync($"///{nameof(View.LoginPage)}");
            }
        }
    }
}