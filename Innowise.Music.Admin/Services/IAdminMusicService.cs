using Innowise.Music.Admin.Models;

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

    // Album operations
    Task<PagedResponse<Album>> GetAllAlbumsAsync(int page = 1, int pageSize = 20);
    Task<Album?> GetAlbumAsync(Guid id);
    Task<Album?> CreateAlbumAsync(Album album);
    Task<Album?> UpdateAlbumAsync(Guid id, Album album);
    Task<bool> DeleteAlbumAsync(Guid id);

    // Track operations
    Task<PagedResponse<Track>> GetAllTracksAsync(int page = 1, int pageSize = 20);
    Task<Track?> GetTrackAsync(Guid id);
    Task<Track?> CreateTrackAsync(Track track);
    Task<Track?> UpdateTrackAsync(Guid id, Track track);
    Task<bool> DeleteTrackAsync(Guid id);
    Task<bool> UploadTrackAudioAsync(Guid trackId, Stream stream, string fileName);
}
