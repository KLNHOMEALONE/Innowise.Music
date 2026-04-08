using Innowise.Music.Services;
using Innowise.Music.View;
using Innowise.Music.ViewModel;
using System;
using System.Threading.Tasks;

namespace Innowise.Music
{
    public partial class AppShell : Shell
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAudioService _audioService;
        private TaskCompletionSource<string>? _authCompletionSource;

        public AppShell(AppShellViewModel viewModel, IAuthenticationService authenticationService, IAudioService audioService)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _authenticationService = authenticationService;
            _audioService = audioService;
            
            _audioService.Initialize(mediaElement);

            Routing.RegisterRoute(nameof(SignUpPage), typeof(SignUpPage));
            Routing.RegisterRoute(nameof(WebPage), typeof(WebPage));

            this.Loaded += OnShellLoaded;
        }

        private async void OnShellLoaded(object sender, EventArgs e)
        {
            // On start, check if the user is authenticated.
            // If they are not, navigate to the login page.
            // If they are, explicitly re-navigate to HomePage to ensure OnAppearing fires
            // and recommendations load (Shell's default tab selection may not reliably
            // trigger OnAppearing on app restart).
            if (!await _authenticationService.IsAuthenticatedAsync())
            {
                await GoToAsync($"///{nameof(LoginPage)}");
            }
            else
            {
                // Force re-navigation to HomePage so OnAppearing fires reliably
                await GoToAsync($"///{nameof(HomePage)}");
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

        public Task<string> GetAuthResultAsync()
        {
            _authCompletionSource = new TaskCompletionSource<string>();
            return _authCompletionSource.Task;
        }

        public void SetAuthResult(string authResult)
        {
            _authCompletionSource?.SetResult(authResult);
        }
    }
}
