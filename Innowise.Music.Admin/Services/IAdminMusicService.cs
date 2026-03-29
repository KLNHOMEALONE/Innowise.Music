using Innowise.MusicIdentityServer.Models.Music;

namespace Innowise.Music.Admin.Services;

public interface IAdminMusicService
{
    // Genre operations
    Task<List<Genre>> GetAllGenresAsync();
    Task<Genre?> GetGenreAsync(Guid id);
    Task<Genre?> CreateGenreAsync(Genre genre);
    Task<Genre?> UpdateGenreAsync(Guid id, Genre genre);
    Task<bool> DeleteGenreAsync(Guid id);

    // Artist operations
    Task<PagedResponse<Artist>> GetAllArtistsAsync(int page = 1, int pageSize = 20);
    Task<Artist?> GetArtistAsync(Guid id);
    Task<Artist?> CreateArtistAsync(Artist artist);
    Task<Artist?> UpdateArtistAsync(Guid id, Artist artist);
    Task<bool> DeleteArtistAsync(Guid id);
    Task<List<Artist>> GetArtistsAsync();

    // Album operations
    Task<PagedResponse<Album>> GetAllAlbumsAsync(int page = 1, int pageSize = 20);
    Task<Album?> GetAlbumAsync(Guid id);
    Task<Album?> CreateAlbumAsync(Album album);
    Task<Album?> UpdateAlbumAsync(Guid id, Album album);
    Task<bool> DeleteAlbumAsync(Guid id);
    Task<List<Album>> GetAlbumsAsync();

    // Track operations
    Task<PagedResponse<Track>> GetAllTracksAsync(int page = 1, int pageSize = 20);
    Task<Track?> GetTrackAsync(Guid id);
    Task<Track?> CreateTrackAsync(Track track);
    Task<Track?> UpdateTrackAsync(Guid id, Track track);
    Task<bool> DeleteTrackAsync(Guid id);
    Task<bool> UploadTrackAudioAsync(Guid trackId, Stream stream, string fileName);
    Task<List<Track>> GetTracksAsync();
}

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
