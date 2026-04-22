using Innowise.Music.Model;
using Innowise.Music.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace Innowise.Music.View;

public partial class EventsPage : ContentPage
{
    private readonly ViewModel.EventsPageViewModel _viewModel;
    private readonly INavigationService _navigationService;
    private readonly Dictionary<Pin, MusicEvent> _pinToEvent = new();

    public EventsPage(ViewModel.EventsPageViewModel viewModel, INavigationService navigationService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _navigationService = navigationService;
        BindingContext = _viewModel;

#if WINDOWS
        // Replace map with placeholder on Windows
        var placeholder = new Label
        {
            Text = "Map is not available on Windows",
            TextColor = Colors.Gray,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            FontSize = 16
        };

        if (MapBorder.Parent is Grid grid)
        {
            grid.Children.Clear();
            grid.Add(placeholder);
        }
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

#if !WINDOWS
        CreatePins();

        // Always center on Warsaw by default
        EventsMap.MoveToRegion(new MapSpan(new Location(52.2297, 21.0122), 0.1, 0.1));

        // Try to get user location and center on it if available
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status == PermissionStatus.Granted)
            {
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location != null)
                {
                    EventsMap.MoveToRegion(new MapSpan(location, 0.1, 0.1));
                }
            }
        }
        catch
        {
            // Permission denied or geolocation failed - Warsaw center already set
        }
#endif
    }

#if !WINDOWS
    private void CreatePins()
    {
        EventsMap.Pins.Clear();
        _pinToEvent.Clear();

        foreach (var group in _viewModel.EventGroups)
        {
            foreach (var evt in group)
            {
                var pin = new Pin
                {
                    Location = new Location(evt.Latitude, evt.Longitude),
                    Label = evt.Title,
                    Address = evt.Venue
                };

                pin.MarkerClicked += OnPinMarkerClicked;
                EventsMap.Pins.Add(pin);
                _pinToEvent[pin] = evt;
            }
        }
    }

    private async void OnPinMarkerClicked(object? sender, PinClickedEventArgs e)
    {
        e.HideInfoWindow = false;

        if (sender is Pin clickedPin && _pinToEvent.TryGetValue(clickedPin, out var evt))
        {
            await DisplayAlert(evt.Title, $"{evt.Date}\n{evt.Venue}", "OK");
        }
    }
#endif

    private void OnCenterOnUserClicked(object sender, EventArgs e)
    {
#if !WINDOWS
        OnAppearing();
#endif
    }
}
