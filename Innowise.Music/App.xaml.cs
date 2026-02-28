namespace Innowise.Music
{
    public partial class App : Application
    {
        private readonly Services.IAuthService _authService;

        public App(Services.IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
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
                //await Shell.Current.GoToAsync($"///{nameof(View.NewsPage)}");
            }
            else
            {
                await Shell.Current.GoToAsync($"///{nameof(View.LoginPage)}");
            }
        }
    }
}