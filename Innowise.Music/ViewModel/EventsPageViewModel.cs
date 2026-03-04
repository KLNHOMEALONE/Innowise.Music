using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        private void LoadMockData()
        {
            var januaryEvents = new EventGroup("January 2026")
            {
                new MusicEvent("Chick Corea", "19 Jan 2026 at 20:00", "Jazz bar, Warsaw", "chick_corea.png"),
                new MusicEvent("Chick Corea", "19 Jan 2026 at 20:00", "Jazz bar, Warsaw", "chick_corea.png")
            };

            var februaryEvents = new EventGroup("February 2026")
            {
                new MusicEvent("Chick Corea", "15 Feb 2026 at 20:00", "Jazz bar, Warsaw", "chick_corea.png")
            };

            EventGroups.Add(januaryEvents);
            EventGroups.Add(februaryEvents);
        }
    }

    public class EventGroup : ObservableCollection<MusicEvent>
    {
        public string Month { get; }

        public EventGroup(string month)
        {
            Month = month;
        }
    }

    public class MusicEvent
    {
        public string Title { get; }
        public string Date { get; }
        public string Venue { get; }
        public string ImageUrl { get; }

        public MusicEvent(string title, string date, string venue, string imageUrl)
        {
            Title = title;
            Date = date;
            Venue = venue;
            ImageUrl = imageUrl;
        }
    }
}
