using Innowise.MusicIdentityServer.Models.Music;

namespace Innowise.MusicIdentityServer.Services;

public interface IMusicService
{
    // Search
    Task<(IEnumerable<Track> Tracks, int TotalCount)> SearchTracksAsync(string query, int page, int pageSize);
    
    // Track operations
    Task<Track?> GetTrackAsync(Guid id);
    Task<Stream?> GetTrackAudioAsync(Guid trackId);
    
    // Artist operations
    Task<Artist?> GetArtistAsync(Guid id);
    Task<IEnumerable<Track>> GetArtistTopTracksAsync(Guid artistId, int count);
    
    // Album operations
    Task<Album?> GetAlbumAsync(Guid id);
    Task<IEnumerable<Track>> GetAlbumTracksAsync(Guid albumId);
}
