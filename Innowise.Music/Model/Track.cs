namespace Innowise.Music.Model
{
    public class Track
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string ArtistName { get; set; }
        public string ImageUrl { get; set; }
        public string FileUri { get; set; }
    }
}
