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

    public async Task<IEnumerable<Track>> GetRecommendedTracksAsync()
    {
        // TODO: Implement actual recommendation algorithm based on user listening history.
        // For now, return a hardcoded set of popular tracks with artist and album data.
        return await _context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .OrderByDescending(t => t.PlayCount)
            .Take(10)
            .ToListAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
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

        if (track.Genres != null && track.Genres.Any())
        {
            var genreIds = track.Genres.Select(g => g.Id).Where(g => g != Guid.Empty).ToList();
            var genres = await _context.Genres.Where(g => genreIds.Contains(g.Id)).ToListAsync();
            track.Genres = genres;
        }
        else
        {
            track.Genres = new List<Genre>();
        }

        _context.Tracks.Add(track);
        await _context.SaveChangesAsync();

        return track;
    }

    public async Task<Track?> UpdateTrackAsync(Guid id, Track track)
    {
        var existingTrack = await _context.Tracks
            .Include(t => t.Genres)
            .FirstOrDefaultAsync(t => t.Id == id);
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

        if (track.Genres != null && track.Genres.Any())
        {
            var genreIds = track.Genres.Select(g => g.Id).Where(g => g != Guid.Empty).ToList();
            var genres = await _context.Genres.Where(g => genreIds.Contains(g.Id)).ToListAsync();
            existingTrack.Genres = genres;
        }
        else
        {
            existingTrack.Genres = new List<Genre>();
        }

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
        // Check if genre with same name already exists
        var existing = await _context.Genres
            .FirstOrDefaultAsync(g => g.Name.ToLower() == genre.Name.ToLower());
        if (existing != null)
        {
            return null;
        }

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

        // Check if another genre with same name already exists
        var duplicate = await _context.Genres
            .FirstOrDefaultAsync(g => g.Name.ToLower() == genre.Name.ToLower() && g.Id != id);
        if (duplicate != null)
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
    
    // ==================== Batch Upload Operations ====================
    
    public async Task<BatchUploadResult> UploadTracksAsync(IEnumerable<TrackUploadDto> tracks)
    {
        var result = new BatchUploadResult();
        
        foreach (var trackDto in tracks)
        {
            try
            {
                // Get or create artist
                Artist? artist = null;
                if (trackDto.ArtistId.HasValue)
                {
                    artist = await _context.Artists.FindAsync(trackDto.ArtistId.Value);
                }
                
                if (artist == null && !string.IsNullOrWhiteSpace(trackDto.ArtistName))
                {
                    artist = await GetOrCreateArtistAsync(trackDto.ArtistName);
                }
                
                if (artist == null)
                {
                    result.Errors.Add($"Failed to create artist: {trackDto.ArtistName}");
                    result.Results.Add(new TrackUploadResult
                    {
                        FileName = trackDto.FileName,
                        Success = false,
                        Error = $"Failed to create artist: {trackDto.ArtistName}"
                    });
                    result.FailedCount++;
                    continue;
                }
                
                // Get or create album (if specified)
                Album? album = null;
                if (trackDto.AlbumId.HasValue)
                {
                    album = await _context.Albums.FindAsync(trackDto.AlbumId.Value);
                }
                
                if (album == null && !string.IsNullOrWhiteSpace(trackDto.AlbumName))
                {
                    album = await GetOrCreateAlbumAsync(trackDto.AlbumName, artist.Id, trackDto.Year);
                }
                
                // Get or create genres
                var genreList = new List<Genre>();
                if (trackDto.GenreIds != null && trackDto.GenreIds.Any())
                {
                    genreList = await _context.Genres
                        .Where(g => trackDto.GenreIds.Contains(g.Id))
                        .ToListAsync();
                }
                else if (trackDto.Genres != null && trackDto.Genres.Any())
                {
                    var genres = await GetOrCreateGenresAsync(trackDto.Genres);
                    genreList.AddRange(genres);
                }
                
                // Create track
                var track = new Track
                {
                    Id = Guid.NewGuid(),
                    Title = trackDto.Title,
                    ArtistId = artist.Id,
                    AlbumId = album?.Id,
                    TrackNumber = trackDto.TrackNumber,
                    Duration = trackDto.Duration,
                    AudioData = trackDto.AudioData,
                    AudioFormat = trackDto.AudioFormat,
                    Bitrate = trackDto.Bitrate,
                    SampleRate = trackDto.SampleRate,
                    FileSize = trackDto.FileSize,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                // Add genres to track
                if (genreList.Any())
                {
                    track.Genres = genreList;
                }
                
                _context.Tracks.Add(track);
                await _context.SaveChangesAsync();
                
                result.Results.Add(new TrackUploadResult
                {
                    FileName = trackDto.FileName,
                    TrackId = track.Id,
                    Success = true
                });
                result.UploadedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading track: {trackDto.FileName}");
                result.Errors.Add($"Error uploading {trackDto.FileName}: {ex.Message}");
                result.Results.Add(new TrackUploadResult
                {
                    FileName = trackDto.FileName,
                    Success = false,
                    Error = ex.Message
                });
                result.FailedCount++;
            }
        }
        
        result.Success = result.UploadedCount > 0;
        return result;
    }
    
    public async Task<Artist?> GetOrCreateArtistAsync(string artistName)
    {
        if (string.IsNullOrWhiteSpace(artistName))
        {
            return null;
        }
        
        // Try to find existing artist
        var existingArtist = await _context.Artists
            .FirstOrDefaultAsync(a => a.Name.ToLower() == artistName.ToLower());
        
        if (existingArtist != null)
        {
            return existingArtist;
        }
        
        // Create new artist
        var newArtist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = artistName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Artists.Add(newArtist);
        await _context.SaveChangesAsync();
        
        return newArtist;
    }
    
    public async Task<Album?> GetOrCreateAlbumAsync(string albumTitle, Guid artistId, uint? year)
    {
        if (string.IsNullOrWhiteSpace(albumTitle))
        {
            return null;
        }
        
        // Try to find existing album for this artist
        var existingAlbum = await _context.Albums
            .FirstOrDefaultAsync(a => a.Title.ToLower() == albumTitle.ToLower() && a.ArtistId == artistId);
        
        if (existingAlbum != null)
        {
            return existingAlbum;
        }
        
        // Create new album
        var newAlbum = new Album
        {
            Id = Guid.NewGuid(),
            Title = albumTitle,
            ArtistId = artistId,
            ReleaseDate = year.HasValue ? new DateOnly((int)year.Value, 1, 1) : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Albums.Add(newAlbum);
        await _context.SaveChangesAsync();
        
        return newAlbum;
    }
    
    public async Task<IEnumerable<Genre>> GetOrCreateGenresAsync(IEnumerable<string> genreNames)
    {
        var genres = new List<Genre>();
        
        if (genreNames == null || !genreNames.Any())
        {
            return genres;
        }
        
        foreach (var genreName in genreNames.Distinct())
        {
            if (string.IsNullOrWhiteSpace(genreName))
            {
                continue;
            }
            
            // Try to find existing genre
            var existingGenre = await _context.Genres
                .FirstOrDefaultAsync(g => g.Name.ToLower() == genreName.ToLower());
            
            if (existingGenre != null)
            {
                genres.Add(existingGenre);
                continue;
            }
            
            // Create new genre
            var newGenre = new Genre
            {
                Id = Guid.NewGuid(),
                Name = genreName
            };
            
            _context.Genres.Add(newGenre);
            genres.Add(newGenre);
        }
        
        await _context.SaveChangesAsync();
        return genres;
    }

    public async Task RecordListeningHistoryAsync(string userId, Guid trackId)
    {
        // Check if track exists
        var track = await _context.Tracks.FindAsync(trackId);
        if (track == null)
        {
            throw new ArgumentException($"Track with id {trackId} not found.", nameof(trackId));
        }

        // Check if already in recent tracks - if so, update PlayedAt to move it to top
        var existing = await _context.UserRecentTracks
            .FirstOrDefaultAsync(r => r.UserId == userId && r.TrackId == trackId);

        if (existing != null)
        {
            existing.PlayedAt = DateTime.UtcNow;
        }
        else
        {
            var recentTrack = new UserRecentTrack
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TrackId = trackId,
                PlayedAt = DateTime.UtcNow
            };
            _context.UserRecentTracks.Add(recentTrack);
        }

        // Trim to keep only the 5 most recent
        var excessEntries = await _context.UserRecentTracks
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.PlayedAt)
            .Skip(5)
            .ToListAsync();

        if (excessEntries.Any())
        {
            _context.UserRecentTracks.RemoveRange(excessEntries);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Track>> GetRecentTracksAsync(string userId, int count)
    {
        var recentTrackIds = await _context.UserRecentTracks
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.PlayedAt)
            .Take(count)
            .Select(r => r.TrackId)
            .ToListAsync();

        return await _context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .Where(t => recentTrackIds.Contains(t.Id))
            .OrderBy(t => recentTrackIds.IndexOf(t.Id))
            .ToListAsync();
    }

    public async Task<IEnumerable<Artist>> GetTopArtistsAsync(string userId, int count)
    {
        return await _context.UserRecentTracks
            .Where(urt => urt.UserId == userId)
            .Include(urt => urt.Track)
            .ThenInclude(t => t.Artist)
            .GroupBy(urt => urt.Track.ArtistId)
            .Select(g => new { ArtistId = g.Key, PlayCount = g.Count() })
            .OrderByDescending(g => g.PlayCount)
            .Take(count)
            .Select(g => g.ArtistId)
            .ToListAsync()
            .ContinueWith(async ids =>
            {
                var artistIds = await ids;
                return await _context.Artists
                    .Where(a => artistIds.Contains(a.Id))
                    .ToListAsync();
            }).Unwrap();
    }
}
