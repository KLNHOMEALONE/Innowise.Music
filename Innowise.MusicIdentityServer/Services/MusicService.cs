using Innowise.MusicIdentityServer.Data;
using Innowise.MusicIdentityServer.Models.Music;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innowise.MusicIdentityServer.Services;

public class MusicService : IMusicService
{
    private readonly MusicIdentityDbContext _context;
    private readonly ILogger<MusicService> _logger;

    public MusicService(MusicIdentityDbContext context, ILogger<MusicService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IEnumerable<Track> Tracks, int TotalCount)> SearchTracksAsync(string query, int page, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return (Enumerable.Empty<Track>(), 0);
        }

        var queryLower = query.ToLower();
        
        var tracksQuery = _context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .Include(t => t.Genres)
            .Where(t => EF.Functions.ILike(t.Title, $"%{queryLower}%") || 
                       EF.Functions.ILike(t.Artist!.Name, $"%{queryLower}%") ||
                       EF.Functions.ILike(t.Album!.Title, $"%{queryLower}%"))
            .OrderByDescending(t => t.PlayCount)
            .ThenBy(t => t.Title);

        var totalCount = await tracksQuery.CountAsync();
        var tracks = await tracksQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (tracks, totalCount);
    }

    public async Task<Track?> GetTrackAsync(Guid id)
    {
        return await _context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .Include(t => t.Genres)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Stream?> GetTrackAudioAsync(Guid trackId)
    {
        var track = await _context.Tracks.FindAsync(trackId);
        if (track?.AudioData == null)
        {
            return null;
        }

        // Increment play count
        track.PlayCount++;
        await _context.SaveChangesAsync();

        return new MemoryStream(track.AudioData);
    }

    public async Task<Artist?> GetArtistAsync(Guid id)
    {
        return await _context.Artists
            .Include(a => a.Tracks)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Track>> GetArtistTopTracksAsync(Guid artistId, int count)
    {
        return await _context.Tracks
            .Where(t => t.ArtistId == artistId)
            .OrderByDescending(t => t.PlayCount)
            .Take(count)
            .Include(t => t.Album)
            .ToListAsync();
    }

    public async Task<Album?> GetAlbumAsync(Guid id)
    {
        return await _context.Albums
            .Include(a => a.Artist)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Track>> GetAlbumTracksAsync(Guid albumId)
    {
        return await _context.Tracks
            .Where(t => t.AlbumId == albumId)
            .OrderBy(t => t.TrackNumber)
            .Include(t => t.Artist)
            .ToListAsync();
    }
}
