using Innowise.MusicIdentityServer.Models.Music;

namespace Innowise.MusicIdentityServer.Services;

public interface IMusicService
{
    // Search
    Task<(IEnumerable<Track> Tracks, int TotalCount)> SearchTracksAsync(string query, int page, int pageSize);
    
    // Track operations
    Task<Track?> GetTrackAsync(Guid id);
    Task<Stream?> GetTrackAudioAsync(Guid trackId);
    Task<IEnumerable<Track>> GetArtistTopTracksAsync(Guid artistId, int count);
    Task<IEnumerable<Track>> GetAlbumTracksAsync(Guid albumId);
    
    // Artist CRUD operations
    Task<IEnumerable<Artist>> GetAllArtistsAsync(int page, int pageSize);
    Task<Artist?> GetArtistAsync(Guid id);
    Task<Artist?> CreateArtistAsync(Artist artist);
    Task<Artist?> UpdateArtistAsync(Guid id, Artist artist);
    Task<bool> DeleteArtistAsync(Guid id);
    
    // Album CRUD operations
    Task<IEnumerable<Album>> GetAllAlbumsAsync(int page, int pageSize);
    Task<Album?> GetAlbumAsync(Guid id);
    Task<Album?> CreateAlbumAsync(Album album);
    Task<Album?> UpdateAlbumAsync(Guid id, Album album);
    Task<bool> DeleteAlbumAsync(Guid id);
    Task<IEnumerable<Album>> GetAlbumsByArtistAsync(Guid artistId);
    
    // Track CRUD operations
    Task<IEnumerable<Track>> GetAllTracksAsync(int page, int pageSize);
    Task<Track?> CreateTrackAsync(Track track);
    Task<Track?> UpdateTrackAsync(Guid id, Track track);
    Task<bool> DeleteTrackAsync(Guid id);
    Task<bool> UploadTrackAudioAsync(Guid trackId, byte[] audioData, string fileName);
    
    // Genre CRUD operations
    Task<IEnumerable<Genre>> GetAllGenresAsync();
    Task<Genre?> GetGenreAsync(Guid id);
    Task<Genre?> CreateGenreAsync(Genre genre);
    Task<Genre?> UpdateGenreAsync(Guid id, Genre genre);
    Task<bool> DeleteGenreAsync(Guid id);
}
