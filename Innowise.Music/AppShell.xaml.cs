using Innowise.Music.View;

namespace Innowise.Music
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(SignUpPage), typeof(SignUpPage));
            Routing.RegisterRoute(nameof(NewsDetailedPage), typeof(NewsDetailedPage));
        }
    }
}
