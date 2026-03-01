using Innowise.Music.Services;

namespace Innowise.Music
{
    public partial class App : Application
    {
        private readonly Services.IAuthenticationService _authenticationService;
        private readonly INavigationService _navigationService;

        public App(Services.IAuthenticationService authenticationService, INavigationService navigationService)
        {
            InitializeComponent();
            _authenticationService = authenticationService;
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
            if (await _authenticationService.IsAuthenticatedAsync())
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