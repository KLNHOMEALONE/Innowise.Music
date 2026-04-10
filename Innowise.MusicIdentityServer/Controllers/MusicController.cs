using Innowise.MusicIdentityServer.Models.Music;
using Innowise.MusicIdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Innowise.MusicIdentityServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MusicController : ControllerBase
{
    private readonly IMusicService _musicService;
    private readonly IStreamTokenService _streamTokenService;
    private readonly ILogger<MusicController> _logger;

    public MusicController(
        IMusicService musicService,
        IStreamTokenService streamTokenService,
        ILogger<MusicController> logger)
    {
        _musicService = musicService;
        _streamTokenService = streamTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Search tracks by title, artist, or album name
    /// </summary>
    [HttpGet("tracks")]
    [Authorize]
    public async Task<ActionResult<SearchTracksResponse>> SearchTracks(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { message = "Query parameter is required" });
        }

        pageSize = Math.Min(pageSize, 50); // Cap at 50
        page = Math.Max(page, 1); // Minimum page 1

        var (tracks, totalCount) = await _musicService.SearchTracksAsync(query, page, pageSize);

        var response = new SearchTracksResponse
        {
            Items = tracks.Select(t => new TrackDto
            {
                Id = t.Id,
                Title = t.Title,
                Duration = t.Duration,
                TrackNumber = t.TrackNumber,
                Artist = t.Artist != null ? new ArtistDto { Id = t.Artist.Id, Name = t.Artist.Name } : null,
                Album = t.Album != null ? new AlbumDto { Id = t.Album.Id, Title = t.Album.Title, CoverImageUrl = t.Album.CoverImageUrl } : null
            }).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(response);
    }

    /// <summary>
    /// Get detailed information about a specific track
    /// </summary>
    [HttpGet("tracks/{id}")]
    [Authorize]
    public async Task<ActionResult<TrackDetailDto>> GetTrack(Guid id)
    {
        var track = await _musicService.GetTrackAsync(id);
        if (track == null)
        {
            return NotFound();
        }

        return Ok(new TrackDetailDto
        {
            Id = track.Id,
            Title = track.Title,
            Duration = track.Duration,
            TrackNumber = track.TrackNumber,
            AudioFormat = track.AudioFormat,
            Bitrate = track.Bitrate,
            SampleRate = track.SampleRate,
            FileSize = track.FileSize,
            ISRC = track.ISRC,
            Explicit = track.Explicit,
            PlayCount = track.PlayCount,
            Artist = track.Artist != null ? new ArtistDto { Id = track.Artist.Id, Name = track.Artist.Name, ImageUrl = track.Artist.ImageUrl } : null,
            Album = track.Album != null ? new AlbumDto 
            { 
                Id = track.Album.Id, 
                Title = track.Album.Title, 
                CoverImageUrl = track.Album.CoverImageUrl,
                ReleaseDate = track.Album.ReleaseDate
            } : null,
            Genres = track.Genres.Select(g => new GenreDto { Id = g.Id, Name = g.Name, Color = g.Color }).ToList()
        });
    }

    /// <summary>
    /// Generate a short-lived signed token for streaming a specific track
    /// </summary>
    [HttpGet("tracks/{id}/stream-token")]
    [Authorize]
    public async Task<ActionResult<StreamTokenResponse>> GetStreamToken(Guid id)
    {
        var track = await _musicService.GetTrackAsync(id);
        if (track == null || track.AudioData == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var token = _streamTokenService.GenerateStreamToken(id, userId);
        return Ok(new StreamTokenResponse { Token = token });
    }

    /// <summary>
    /// Stream audio for a specific track (supports range requests)
    /// Accepts either a JWT Bearer token or a signed stream token via query parameter
    /// </summary>
    [HttpGet("tracks/{id}/stream")]
    public async Task<IActionResult> StreamTrack(Guid id, [FromQuery] string? token)
    {
        // Validate access: either via signed stream token (query param) or JWT Bearer (Authorization header)
        if (!string.IsNullOrEmpty(token))
        {
            if (!_streamTokenService.ValidateStreamToken(token, out var trackId) || trackId != id)
            {
                return Unauthorized();
            }
        }
        else
        {
            // Fall back to standard JWT authorization
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized();
            }
        }

        var track = await _musicService.GetTrackAsync(id);
        if (track == null || track.AudioData == null)
        {
            return NotFound();
        }

        // Increment play count for streaming
        track.PlayCount++;
        await _musicService.SaveChangesAsync();

        var audioStream = await _musicService.GetTrackAudioAsync(id);
        if (audioStream == null)
        {
            return NotFound();
        }

        var contentType = track.AudioFormat.ToLower() switch
        {
            "mp3" => "audio/mpeg",
            "wav" => "audio/wav",
            "flac" => "audio/flac",
            "aac" => "audio/aac",
            _ => "application/octet-stream"
        };

        Response.Headers.AcceptRanges = "bytes";
        Response.Headers.CacheControl = "public, max-age=31536000";

        return File(audioStream, contentType, enableRangeProcessing: true);
    }

    /// <summary>
    /// Get top tracks for a specific artist
    /// </summary>
    [HttpGet("artists/{id}/top-tracks")]
    [Authorize]
    public async Task<ActionResult<ArtistTopTracksResponse>> GetArtistTopTracks(
        Guid id, 
        [FromQuery] int count = 10)
    {
        var artist = await _musicService.GetArtistAsync(id);
        if (artist == null)
        {
            return NotFound();
        }

        count = Math.Min(count, 50); // Cap at 50
        var tracks = await _musicService.GetArtistTopTracksAsync(id, count);

        return Ok(new ArtistTopTracksResponse
        {
            Artist = new ArtistDto { Id = artist.Id, Name = artist.Name, ImageUrl = artist.ImageUrl },
            Tracks = tracks.Select(t => new TrackDto
            {
                Id = t.Id,
                Title = t.Title,
                Duration = t.Duration,
                PlayCount = t.PlayCount,
                Album = t.Album != null ? new AlbumDto { Id = t.Album.Id, Title = t.Album.Title, CoverImageUrl = t.Album.CoverImageUrl } : null
            }).ToList()
        });
    }

    /// <summary>
    /// Get personalized track recommendations for the authenticated user
    /// </summary>
    [HttpGet("recommendations")]
    [Authorize]
    public async Task<ActionResult<RecommendationsResponse>> GetRecommendations()
    {
        // TODO: Replace with actual recommendation algorithm based on user listening history
        var tracks = await _musicService.GetRecommendedTracksAsync();

        return Ok(new RecommendationsResponse
        {
            Tracks = tracks.Select(t => new TrackDto
            {
                Id = t.Id,
                Title = t.Title,
                Duration = t.Duration,
                PlayCount = t.PlayCount,
                Artist = t.Artist != null ? new ArtistDto { Id = t.Artist.Id, Name = t.Artist.Name, ImageUrl = t.Artist.ImageUrl } : null,
                Album = t.Album != null ? new AlbumDto { Id = t.Album.Id, Title = t.Album.Title, CoverImageUrl = t.Album.CoverImageUrl } : null
            }).ToList()
        });
    }

    /// <summary>
    /// Get top artists from the authenticated user's listening history
    /// </summary>
    [HttpGet("recommendations/artists")]
    [Authorize]
    public async Task<ActionResult<List<ArtistDto>>> GetRecommendedArtists()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var artists = await _musicService.GetTopArtistsAsync(userId, 3);

        return Ok(artists.Select(a => new ArtistDto
        {
            Id = a.Id,
            Name = a.Name,
            ImageUrl = a.ImageUrl
        }).ToList());
    }

    /// <summary>
    /// Get all tracks from an album
    /// </summary>
    [HttpGet("albums/{id}/tracks")]
    [Authorize]
    public async Task<ActionResult<AlbumTracksResponse>> GetAlbumTracks(Guid id)
    {
        var album = await _musicService.GetAlbumAsync(id);
        if (album == null)
        {
            return NotFound();
        }

        var tracks = await _musicService.GetAlbumTracksAsync(id);

        return Ok(new AlbumTracksResponse
        {
            Album = new AlbumDto
            {
                Id = album.Id,
                Title = album.Title,
                CoverImageUrl = album.CoverImageUrl,
                ReleaseDate = album.ReleaseDate
            },
            Tracks = tracks.Select(t => new TrackDto
            {
                Id = t.Id,
                Title = t.Title,
                Duration = t.Duration,
                TrackNumber = t.TrackNumber,
                Artist = t.Artist != null ? new ArtistDto { Id = t.Artist.Id, Name = t.Artist.Name } : null
            }).ToList(),
            TotalDuration = tracks.Sum(t => t.Duration)
        });
    }

    /// <summary>
    /// Record that the authenticated user played a track
    /// </summary>
    [HttpPost("tracks/{id}/history")]
    [Authorize]
    public async Task<IActionResult> RecordHistory(Guid id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            await _musicService.RecordListeningHistoryAsync(userId, id);
            return Ok();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error recording history for user {userId}, track {id}");
            return StatusCode(500, new { message = "Failed to record listening history" });
        }
    }

    /// <summary>
    /// Get recent tracks for the authenticated user
    /// </summary>
    [HttpGet("history/recent")]
    [Authorize]
    public async Task<ActionResult<RecentTracksResponse>> GetRecentTracks(
        [FromQuery] int count = 5)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        count = Math.Min(count, 10); // Cap at 10
        var tracks = await _musicService.GetRecentTracksAsync(userId, count);

        return Ok(new RecentTracksResponse
        {
            Tracks = tracks.Select(t => new TrackDto
            {
                Id = t.Id,
                Title = t.Title,
                Duration = t.Duration,
                TrackNumber = t.TrackNumber,
                PlayCount = t.PlayCount,
                Artist = t.Artist != null ? new ArtistDto { Id = t.Artist.Id, Name = t.Artist.Name, ImageUrl = t.Artist.ImageUrl } : null,
                Album = t.Album != null ? new AlbumDto { Id = t.Album.Id, Title = t.Album.Title, CoverImageUrl = t.Album.CoverImageUrl } : null
            }).ToList()
        });
    }

    /// <summary>
    /// Toggle favorite status for a track (add if not favorite, remove if favorite)
    /// </summary>
    [HttpPost("tracks/{id}/favorite")]
    [Authorize]
    public async Task<IActionResult> ToggleFavorite(Guid id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var isFavorite = await _musicService.ToggleFavoriteAsync(userId, id);
            return Ok(new { isFavorite });
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error toggling favorite for user {userId}, track {id}");
            return StatusCode(500, new { message = "Failed to toggle favorite" });
        }
    }

    /// <summary>
    /// Check if a track is favorited by the authenticated user
    /// </summary>
    [HttpGet("tracks/{id}/is-favorite")]
    [Authorize]
    public async Task<IActionResult> IsFavorite(Guid id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var isFavorite = await _musicService.IsFavoriteAsync(userId, id);
            return Ok(new { isFavorite });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking favorite status for user {userId}, track {id}");
            return StatusCode(500, new { message = "Failed to check favorite status" });
        }
    }

    /// <summary>
    /// Get all favorite tracks for the authenticated user
    /// </summary>
    [HttpGet("favorites")]
    [Authorize]
    public async Task<ActionResult<RecentTracksResponse>> GetFavorites()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var tracks = await _musicService.GetFavoritesAsync(userId);

            return Ok(new RecentTracksResponse
            {
                Tracks = tracks.Select(t => new TrackDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Duration = t.Duration,
                    TrackNumber = t.TrackNumber,
                    PlayCount = t.PlayCount,
                    Artist = t.Artist != null ? new ArtistDto { Id = t.Artist.Id, Name = t.Artist.Name, ImageUrl = t.Artist.ImageUrl } : null,
                    Album = t.Album != null ? new AlbumDto { Id = t.Album.Id, Title = t.Album.Title, CoverImageUrl = t.Album.CoverImageUrl } : null
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting favorites for user {userId}");
            return StatusCode(500, new { message = "Failed to get favorites" });
        }
    }

    // DTOs for responses
    public class SearchTracksResponse
    {
        public List<TrackDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class TrackDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int? TrackNumber { get; set; }
        public long PlayCount { get; set; }
        public ArtistDto? Artist { get; set; }
        public AlbumDto? Album { get; set; }
    }

    public class TrackDetailDto : TrackDto
    {
        public string AudioFormat { get; set; } = string.Empty;
        public int? Bitrate { get; set; }
        public int? SampleRate { get; set; }
        public long? FileSize { get; set; }
        public string? ISRC { get; set; }
        public bool Explicit { get; set; }
        public List<GenreDto> Genres { get; set; } = new();
    }

    public class ArtistDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    public class AlbumDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public DateOnly? ReleaseDate { get; set; }
    }

    public class GenreDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
    }

    public class ArtistTopTracksResponse
    {
        public ArtistDto Artist { get; set; } = new();
        public List<TrackDto> Tracks { get; set; } = new();
    }

    public class StreamTokenResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    public class RecommendationsResponse
    {
        public List<TrackDto> Tracks { get; set; } = new();
    }

    public class AlbumTracksResponse
    {
        public AlbumDto Album { get; set; } = new();
        public List<TrackDto> Tracks { get; set; } = new();
        public int TotalDuration { get; set; }
    }

    public class RecentTracksResponse
    {
        public List<TrackDto> Tracks { get; set; } = new();
    }
}
