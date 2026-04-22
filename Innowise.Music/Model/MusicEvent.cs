namespace Innowise.Music.Model
{
    public class MusicEvent
    {
        public string Title { get; }
        public string Date { get; }
        public string Venue { get; }
        public string ImageUrl { get; }
        public double Latitude { get; }
        public double Longitude { get; }

        public MusicEvent(string title, string date, string venue, string imageUrl, double latitude, double longitude)
        {
            Title = title;
            Date = date;
            Venue = venue;
            ImageUrl = imageUrl;
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
