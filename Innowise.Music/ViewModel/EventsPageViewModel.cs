using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Innowise.Music.Model;
using Microsoft.Maui.Devices.Sensors;
using System.Collections.ObjectModel;

namespace Innowise.Music.ViewModel
{
    public partial class EventsPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isMapViewVisible = true;

        [ObservableProperty]
        private bool _isListViewVisible = false;

        public ObservableCollection<EventGroup> EventGroups { get; } = new();

        public EventsPageViewModel()
        {
            LoadMockData();
        }

        [RelayCommand]
        private void ShowMapView()
        {
            IsMapViewVisible = true;
            IsListViewVisible = false;
        }

        [RelayCommand]
        private void ShowListView()
        {
            IsMapViewVisible = false;
            IsListViewVisible = true;
        }

        partial void OnIsMapViewVisibleChanged(bool value)
        {
            if (value)
                IsListViewVisible = false;
        }

        partial void OnIsListViewVisibleChanged(bool value)
        {
            if (value)
                IsMapViewVisible = false;
        }

        private void LoadMockData()
        {
            var januaryEvents = new EventGroup("January 2026")
            {
                new MusicEvent("Chick Corea Trio", "19 Jan 2026 at 20:00", "Jazz bar, Warsaw", "chick_corea.png", 52.2297, 21.0122),
                new MusicEvent("Chick Corea Trio", "19 Jan 2026 at 20:00", "Jazz bar, Warsaw", "chick_corea.png", 52.2297, 21.0122)
            };

            var februaryEvents = new EventGroup("February 2026")
            {
                new MusicEvent("Chick Corea Trio", "15 Feb 2026 at 20:00", "Jazz bar, Warsaw", "chick_corea.png", 52.2297, 21.0122)
            };

            EventGroups.Add(januaryEvents);
            EventGroups.Add(februaryEvents);
        }
    }
}
