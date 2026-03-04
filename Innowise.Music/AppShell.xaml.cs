using Innowise.Music.Services;
using Innowise.Music.View;
using Innowise.Music.ViewModel;

namespace Innowise.Music
{
    public partial class AppShell : Shell
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly INavigationService _navigationService;

        public AppShell(AppShellViewModel viewModel, IAuthenticationService authenticationService, INavigationService navigationService)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _authenticationService = authenticationService;
            _navigationService = navigationService;
            Routing.RegisterRoute(nameof(SignUpPage), typeof(SignUpPage));
            Routing.RegisterRoute(nameof(NewsDetailedPage), typeof(NewsDetailedPage));
        }

        protected override async void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);

            if (args.Source == ShellNavigationSource.ShellSectionChanged)
            {
                var viewModel = (AppShellViewModel)BindingContext;
                viewModel.SelectedRoute = args.Target.Location.OriginalString;

                if (await _authenticationService.IsAuthenticatedAsync())
                {
                    // User is authenticated
                }
                else
                {
                    // User is not authenticated, redirect to login page
                    await _navigationService.NavigateToAsync($"///{nameof(View.LoginPage)}");
                }
            }
        }
    }
}
