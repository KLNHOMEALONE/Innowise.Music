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

    public async Task<IEnumerable<Track>> GetArtistTopTracksAsync(Guid artistId, int count)
    {
        return await _context.Tracks
            .Where(t => t.ArtistId == artistId)
            .OrderByDescending(t => t.PlayCount)
            .Take(count)
            .Include(t => t.Album)
            .ToListAsync();
    }

    public async Task<IEnumerable<Track>> GetAlbumTracksAsync(Guid albumId)
    {
        return await _context.Tracks
            .Where(t => t.AlbumId == albumId)
            .OrderBy(t => t.TrackNumber)
            .Include(t => t.Artist)
            .ToListAsync();
    }

    // ==================== Artist CRUD Operations ====================

    public async Task<IEnumerable<Artist>> GetAllArtistsAsync(int page, int pageSize)
    {
        return await _context.Artists
            .OrderBy(a => a.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetArtistsCountAsync()
    {
        return await _context.Artists.CountAsync();
    }

    public async Task<Artist?> GetArtistAsync(Guid id)
    {
        return await _context.Artists
            .Include(a => a.Albums)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Artist?> CreateArtistAsync(Artist artist)
    {
        artist.Id = Guid.NewGuid();
        artist.CreatedAt = DateTime.UtcNow;
        artist.UpdatedAt = DateTime.UtcNow;

        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();

        return artist;
    }

    public async Task<Artist?> UpdateArtistAsync(Guid id, Artist artist)
    {
        var existingArtist = await _context.Artists.FindAsync(id);
        if (existingArtist == null)
        {
            return null;
        }

        existingArtist.Name = artist.Name;
        existingArtist.Biography = artist.Biography;
        existingArtist.ImageUrl = artist.ImageUrl;
        existingArtist.Verified = artist.Verified;
        existingArtist.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return existingArtist;
    }

    public async Task<bool> DeleteArtistAsync(Guid id)
    {
        var artist = await _context.Artists.FindAsync(id);
        if (artist == null)
        {
            return false;
        }

        _context.Artists.Remove(artist);
        await _context.SaveChangesAsync();

        return true;
    }

    // ==================== Album CRUD Operations ====================

    public async Task<IEnumerable<Album>> GetAllAlbumsAsync(int page, int pageSize)
    {
        return await _context.Albums
            .Include(a => a.Artist)
            .OrderByDescending(a => a.ReleaseDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetAlbumsCountAsync()
    {
        return await _context.Albums.CountAsync();
    }

    public async Task<Album?> GetAlbumAsync(Guid id)
    {
        return await _context.Albums
            .Include(a => a.Artist)
            .Include(a => a.Tracks)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Album?> CreateAlbumAsync(Album album)
    {
        album.Id = Guid.NewGuid();
        album.CreatedAt = DateTime.UtcNow;
        album.UpdatedAt = DateTime.UtcNow;

        _context.Albums.Add(album);
        await _context.SaveChangesAsync();

        return album;
    }

    public async Task<Album?> UpdateAlbumAsync(Guid id, Album album)
    {
        var existingAlbum = await _context.Albums.FindAsync(id);
        if (existingAlbum == null)
        {
            return null;
        }

        existingAlbum.Title = album.Title;
        existingAlbum.ArtistId = album.ArtistId;
        existingAlbum.ReleaseDate = album.ReleaseDate;
        existingAlbum.CoverImageUrl = album.CoverImageUrl;
        existingAlbum.Genre = album.Genre;
        existingAlbum.Label = album.Label;
        existingAlbum.Duration = album.Duration;
        existingAlbum.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return existingAlbum;
    }

    public async Task<bool> DeleteAlbumAsync(Guid id)
    {
        var album = await _context.Albums.FindAsync(id);
        if (album == null)
        {
            return false;
        }

        _context.Albums.Remove(album);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<Album>> GetAlbumsByArtistAsync(Guid artistId)
    {
        return await _context.Albums
            .Where(a => a.ArtistId == artistId)
            .OrderByDescending(a => a.ReleaseDate)
            .Include(a => a.Artist)
            .ToListAsync();
    }

    // ==================== Track CRUD Operations ====================

    public async Task<IEnumerable<Track>> GetAllTracksAsync(int page, int pageSize)
    {
        return await _context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .Include(t => t.Genres)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTracksCountAsync()
    {
        return await _context.Tracks.CountAsync();
    }

    public async Task<Track?> CreateTrackAsync(Track track)
    {
        track.Id = Guid.NewGuid();
        track.CreatedAt = DateTime.UtcNow;
        track.UpdatedAt = DateTime.UtcNow;

        _context.Tracks.Add(track);
        await _context.SaveChangesAsync();

        return track;
    }

    public async Task<Track?> UpdateTrackAsync(Guid id, Track track)
    {
        var existingTrack = await _context.Tracks.FindAsync(id);
        if (existingTrack == null)
        {
            return null;
        }

        existingTrack.Title = track.Title;
        existingTrack.ArtistId = track.ArtistId;
        existingTrack.AlbumId = track.AlbumId;
        existingTrack.TrackNumber = track.TrackNumber;
        existingTrack.Duration = track.Duration;
        existingTrack.AudioFormat = track.AudioFormat;
        existingTrack.Bitrate = track.Bitrate;
        existingTrack.SampleRate = track.SampleRate;
        existingTrack.FileSize = track.FileSize;
        existingTrack.ISRC = track.ISRC;
        existingTrack.Explicit = track.Explicit;
        existingTrack.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return existingTrack;
    }

    public async Task<bool> DeleteTrackAsync(Guid id)
    {
        var track = await _context.Tracks.FindAsync(id);
        if (track == null)
        {
            return false;
        }

        _context.Tracks.Remove(track);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UploadTrackAudioAsync(Guid trackId, byte[] audioData, string fileName)
    {
        var track = await _context.Tracks.FindAsync(trackId);
        if (track == null)
        {
            return false;
        }

        track.AudioData = audioData;
        track.FileSize = audioData.Length;

        // Extract audio format from file extension
        var extension = Path.GetExtension(fileName).ToLower();
        track.AudioFormat = extension switch
        {
            ".mp3" => "MP3",
            ".wav" => "WAV",
            ".flac" => "FLAC",
            ".aac" => "AAC",
            _ => "UNKNOWN"
        };

        track.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    // ==================== Genre CRUD Operations ====================

    public async Task<IEnumerable<Genre>> GetAllGenresAsync()
    {
        return await _context.Genres
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<Genre?> GetGenreAsync(Guid id)
    {
        return await _context.Genres
            .Include(g => g.Tracks)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Genre?> CreateGenreAsync(Genre genre)
    {
        genre.Id = Guid.NewGuid();

        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();

        return genre;
    }

    public async Task<Genre?> UpdateGenreAsync(Guid id, Genre genre)
    {
        var existingGenre = await _context.Genres.FindAsync(id);
        if (existingGenre == null)
        {
            return null;
        }

        existingGenre.Name = genre.Name;
        existingGenre.Description = genre.Description;
        existingGenre.ImageUrl = genre.ImageUrl;
        existingGenre.Color = genre.Color;

        await _context.SaveChangesAsync();

        return existingGenre;
    }

    public async Task<bool> DeleteGenreAsync(Guid id)
    {
        var genre = await _context.Genres.FindAsync(id);
        if (genre == null)
        {
            return false;
        }

        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync();

        return true;
    }
}
