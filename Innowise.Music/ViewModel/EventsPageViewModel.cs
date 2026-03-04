using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Innowise.Music.ViewModel;

public partial class EventsPageViewModel : ObservableObject
{
    public ObservableCollection<string> Categories { get; } = new()
    {
        "Concerts", "Festivals", "Clubs", "Nearby"
    };

    public ObservableCollection<MusicEvent> UpcomingEvents { get; } = new()
    {
        new MusicEvent("Invent Animate - Heavener Tour", "March 15, 2026", "The Masquerade, Atlanta", "https://example.com/event1.jpg"),
        new MusicEvent("Silent Planet - SUPERBLOOM", "March 22, 2026", "Tabernacle, Atlanta", "https://example.com/event2.jpg"),
        new MusicEvent("Metalcore Festival 2026", "April 10-12, 2026", "Georgia International Convention Center", "https://example.com/festival.jpg"),
        new MusicEvent("Currents - The Death We Seek", "May 05, 2026", "Buckhead Theatre, Atlanta", "https://example.com/event3.jpg")
    };

    public EventsPageViewModel()
    {
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
