using Innowise.MusicIdentityServer.Models.Music;

namespace Innowise.MusicIdentityServer.Services;

public interface IMusicService
{
    // Search
    Task<(IEnumerable<Track> Tracks, int TotalCount)> SearchTracksAsync(string query, int skip, int take);
    Task<(IEnumerable<Artist> Artists, int TotalCount)> SearchArtistsAsync(string query, int skip, int take);
    Task<(IEnumerable<Album> Albums, int TotalCount)> SearchAlbumsAsync(string query, int skip, int take);
    
    // Track operations
    Task<Track?> GetTrackAsync(Guid id);
    Task<Stream?> GetTrackAudioAsync(Guid trackId);
    Task<IEnumerable<Track>> GetArtistTopTracksAsync(Guid artistId, int count);
    Task<IEnumerable<Track>> GetAlbumTracksAsync(Guid albumId);
    Task<IEnumerable<Track>> GetRecommendedTracksAsync();
    Task<int> SaveChangesAsync();
    
    // Artist CRUD operations
    Task<IEnumerable<Artist>> GetAllArtistsAsync(int page, int pageSize);
    Task<int> GetArtistsCountAsync();
    Task<Artist?> GetArtistAsync(Guid id);
    Task<Artist?> CreateArtistAsync(Artist artist);
    Task<Artist?> UpdateArtistAsync(Guid id, Artist artist);
    Task<bool> DeleteArtistAsync(Guid id);
    
    // Album CRUD operations
    Task<IEnumerable<Album>> GetAllAlbumsAsync(int page, int pageSize);
    Task<int> GetAlbumsCountAsync();
    Task<Album?> GetAlbumAsync(Guid id);
    Task<Album?> CreateAlbumAsync(Album album);
    Task<Album?> UpdateAlbumAsync(Guid id, Album album);
    Task<bool> DeleteAlbumAsync(Guid id);
    Task<IEnumerable<Album>> GetAlbumsByArtistAsync(Guid artistId);
    
    // Track CRUD operations
    Task<IEnumerable<Track>> GetAllTracksAsync(int page, int pageSize);
    Task<int> GetTracksCountAsync();
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
    
    // Batch upload operations
    Task<BatchUploadResult> UploadTracksAsync(IEnumerable<TrackUploadDto> tracks);
    Task<Artist?> GetOrCreateArtistAsync(string artistName);
    Task<Album?> GetOrCreateAlbumAsync(string albumTitle, Guid artistId, uint? year);
    Task<IEnumerable<Genre>> GetOrCreateGenresAsync(IEnumerable<string> genreNames);

    // Listening history operations
    Task RecordListeningHistoryAsync(string userId, Guid trackId);
    Task<IEnumerable<Track>> GetRecentTracksAsync(string userId, int count);
    Task<IEnumerable<Artist>> GetTopArtistsAsync(string userId, int count);

    // Favorite operations
    Task<bool> ToggleFavoriteAsync(string userId, Guid trackId);
    Task<bool> IsFavoriteAsync(string userId, Guid trackId);
    Task<IEnumerable<Track>> GetFavoritesAsync(string userId);
}
