using Innowise.Music.Services;
using Innowise.Music.View;
using Innowise.Music.ViewModel;
using System;

namespace Innowise.Music
{
    public partial class AppShell : Shell
    {
        private readonly IAuthenticationService _authenticationService;

        public AppShell(AppShellViewModel viewModel, IAuthenticationService authenticationService)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _authenticationService = authenticationService;
            Routing.RegisterRoute(nameof(SignUpPage), typeof(SignUpPage));
            Routing.RegisterRoute(nameof(NewsDetailedPage), typeof(NewsDetailedPage));

            this.Loaded += OnShellLoaded;
        }

        private async void OnShellLoaded(object sender, EventArgs e)
        {
            // On start, check if the user is authenticated.
            // If they are not, navigate to the login page.
            // Otherwise, let the Shell default to the main TabBar content (HomePage).
            if (!await _authenticationService.IsAuthenticatedAsync())
            {
                await GoToAsync($"///{nameof(LoginPage)}");
            }
        }

        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);

            if (args.Source == ShellNavigationSource.ShellSectionChanged)
            {
                var viewModel = (AppShellViewModel)BindingContext;
                if (args.Target.Location.OriginalString != null)
                {
                    viewModel.SelectedRoute = args.Target.Location.OriginalString;
                }
            }
        }
    }
}
