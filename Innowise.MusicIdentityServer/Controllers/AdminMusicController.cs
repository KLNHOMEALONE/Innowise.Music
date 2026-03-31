using Innowise.MusicIdentityServer.Models.Music;
using Innowise.MusicIdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Innowise.MusicIdentityServer.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Administrator")]
public class AdminMusicController : ControllerBase
{
    private readonly IMusicService _musicService;
    private readonly ILogger<AdminMusicController> _logger;

    public AdminMusicController(IMusicService musicService, ILogger<AdminMusicController> logger)
    {
        _musicService = musicService;
        _logger = logger;
    }

    // ==================== Genre Endpoints ====================

    /// <summary>
    /// Get all genres
    /// </summary>
    [HttpGet("genres")]
    public async Task<ActionResult<IEnumerable<Genre>>> GetGenres()
    {
        var genres = await _musicService.GetAllGenresAsync();
        return Ok(genres);
    }

    /// <summary>
    /// Get genre by ID
    /// </summary>
    [HttpGet("genres/{id:guid}")]
    public async Task<ActionResult<Genre>> GetGenre(Guid id)
    {
        var genre = await _musicService.GetGenreAsync(id);
        if (genre == null)
        {
            return NotFound();
        }
        return Ok(genre);
    }

    /// <summary>
    /// Create new genre
    /// </summary>
    [HttpPost("genres")]
    public async Task<ActionResult<Genre>> CreateGenre(Genre genre)
    {
        if (string.IsNullOrWhiteSpace(genre.Name))
        {
            return BadRequest(new { message = "Genre name is required" });
        }

        var createdGenre = await _musicService.CreateGenreAsync(genre);
        return CreatedAtAction(nameof(GetGenre), new { id = createdGenre.Id }, createdGenre);
    }

    /// <summary>
    /// Update genre
    /// </summary>
    [HttpPut("genres/{id:guid}")]
    public async Task<IActionResult> UpdateGenre(Guid id, Genre genre)
    {
        if (string.IsNullOrWhiteSpace(genre.Name))
        {
            return BadRequest(new { message = "Genre name is required" });
        }

        var updatedGenre = await _musicService.UpdateGenreAsync(id, genre);
        if (updatedGenre == null)
        {
            return NotFound();
        }
        return Ok(updatedGenre);
    }

    /// <summary>
    /// Delete genre
    /// </summary>
    [HttpDelete("genres/{id:guid}")]
    public async Task<IActionResult> DeleteGenre(Guid id)
    {
        var result = await _musicService.DeleteGenreAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    // ==================== Artist Endpoints ====================

    /// <summary>
    /// Get all artists with pagination
    /// </summary>
    [HttpGet("artists")]
    public async Task<ActionResult<PagedResponse<Artist>>> GetArtists(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Min(pageSize, 100); // Cap at 100
        page = Math.Max(page, 1); // Minimum page 1

        var artists = await _musicService.GetAllArtistsAsync(page, pageSize);
        var totalCount = await _musicService.GetArtistsCountAsync();

        return Ok(new PagedResponse<Artist>
        {
            Items = artists,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// Get artist by ID
    /// </summary>
    [HttpGet("artists/{id:guid}")]
    public async Task<ActionResult<Artist>> GetArtist(Guid id)
    {
        var artist = await _musicService.GetArtistAsync(id);
        if (artist == null)
        {
            return NotFound();
        }
        return Ok(artist);
    }

    /// <summary>
    /// Create new artist
    /// </summary>
    [HttpPost("artists")]
    public async Task<ActionResult<Artist>> CreateArtist(Artist artist)
    {
        if (string.IsNullOrWhiteSpace(artist.Name))
        {
            return BadRequest(new { message = "Artist name is required" });
        }

        var createdArtist = await _musicService.CreateArtistAsync(artist);
        return CreatedAtAction(nameof(GetArtist), new { id = createdArtist.Id }, createdArtist);
    }

    /// <summary>
    /// Update artist
    /// </summary>
    [HttpPut("artists/{id:guid}")]
    public async Task<IActionResult> UpdateArtist(Guid id, Artist artist)
    {
        if (string.IsNullOrWhiteSpace(artist.Name))
        {
            return BadRequest(new { message = "Artist name is required" });
        }

        var updatedArtist = await _musicService.UpdateArtistAsync(id, artist);
        if (updatedArtist == null)
        {
            return NotFound();
        }
        return Ok(updatedArtist);
    }

    /// <summary>
    /// Delete artist
    /// </summary>
    [HttpDelete("artists/{id:guid}")]
    public async Task<IActionResult> DeleteArtist(Guid id)
    {
        var result = await _musicService.DeleteArtistAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    // ==================== Album Endpoints ====================

    /// <summary>
    /// Get all albums with pagination
    /// </summary>
    [HttpGet("albums")]
    public async Task<ActionResult<PagedResponse<Album>>> GetAlbums(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Min(pageSize, 100);
        page = Math.Max(page, 1);

        var albums = await _musicService.GetAllAlbumsAsync(page, pageSize);
        var totalCount = await _musicService.GetAlbumsCountAsync();

        return Ok(new PagedResponse<Album>
        {
            Items = albums,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// Get album by ID
    /// </summary>
    [HttpGet("albums/{id:guid}")]
    public async Task<ActionResult<Album>> GetAlbum(Guid id)
    {
        var album = await _musicService.GetAlbumAsync(id);
        if (album == null)
        {
            return NotFound();
        }
        return Ok(album);
    }

    /// <summary>
    /// Create new album
    /// </summary>
    [HttpPost("albums")]
    public async Task<ActionResult<Album>> CreateAlbum(Album album)
    {
        if (string.IsNullOrWhiteSpace(album.Title))
        {
            return BadRequest(new { message = "Album title is required" });
        }

        var createdAlbum = await _musicService.CreateAlbumAsync(album);
        return CreatedAtAction(nameof(GetAlbum), new { id = createdAlbum.Id }, createdAlbum);
    }

    /// <summary>
    /// Update album
    /// </summary>
    [HttpPut("albums/{id:guid}")]
    public async Task<IActionResult> UpdateAlbum(Guid id, Album album)
    {
        if (string.IsNullOrWhiteSpace(album.Title))
        {
            return BadRequest(new { message = "Album title is required" });
        }

        var updatedAlbum = await _musicService.UpdateAlbumAsync(id, album);
        if (updatedAlbum == null)
        {
            return NotFound();
        }
        return Ok(updatedAlbum);
    }

    /// <summary>
    /// Delete album
    /// </summary>
    [HttpDelete("albums/{id:guid}")]
    public async Task<IActionResult> DeleteAlbum(Guid id)
    {
        var result = await _musicService.DeleteAlbumAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Get albums by artist
    /// </summary>
    [HttpGet("artists/{artistId:guid}/albums")]
    public async Task<ActionResult<IEnumerable<Album>>> GetAlbumsByArtist(Guid artistId)
    {
        var albums = await _musicService.GetAlbumsByArtistAsync(artistId);
        return Ok(albums);
    }

    // ==================== Track Endpoints ====================

    /// <summary>
    /// Get all tracks with pagination
    /// </summary>
    [HttpGet("tracks")]
    public async Task<ActionResult<PagedResponse<Track>>> GetTracks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Min(pageSize, 100);
        page = Math.Max(page, 1);

        var tracks = await _musicService.GetAllTracksAsync(page, pageSize);
        var totalCount = await _musicService.GetTracksCountAsync();

        return Ok(new PagedResponse<Track>
        {
            Items = tracks,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// Get track by ID
    /// </summary>
    [HttpGet("tracks/{id:guid}")]
    public async Task<ActionResult<Track>> GetTrack(Guid id)
    {
        var track = await _musicService.GetTrackAsync(id);
        if (track == null)
        {
            return NotFound();
        }
        return Ok(track);
    }

    /// <summary>
    /// Create new track (metadata only)
    /// </summary>
    [HttpPost("tracks")]
    public async Task<ActionResult<Track>> CreateTrack(Track track)
    {
        if (string.IsNullOrWhiteSpace(track.Title))
        {
            return BadRequest(new { message = "Track title is required" });
        }

        if (track.Duration <= 0)
        {
            return BadRequest(new { message = "Track duration must be greater than 0" });
        }

        var createdTrack = await _musicService.CreateTrackAsync(track);
        return CreatedAtAction(nameof(GetTrack), new { id = createdTrack.Id }, createdTrack);
    }

    /// <summary>
    /// Update track
    /// </summary>
    [HttpPut("tracks/{id:guid}")]
    public async Task<IActionResult> UpdateTrack(Guid id, Track track)
    {
        if (string.IsNullOrWhiteSpace(track.Title))
        {
            return BadRequest(new { message = "Track title is required" });
        }

        if (track.Duration <= 0)
        {
            return BadRequest(new { message = "Track duration must be greater than 0" });
        }

        var updatedTrack = await _musicService.UpdateTrackAsync(id, track);
        if (updatedTrack == null)
        {
            return NotFound();
        }
        return Ok(updatedTrack);
    }

    /// <summary>
    /// Delete track
    /// </summary>
    [HttpDelete("tracks/{id:guid}")]
    public async Task<IActionResult> DeleteTrack(Guid id)
    {
        var result = await _musicService.DeleteTrackAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Upload audio file for track
    /// </summary>
    [HttpPost("tracks/{id:guid}/upload")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB limit
    public async Task<IActionResult> UploadAudio(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded" });
        }

        // Validate file type
        var allowedExtensions = new[] { ".mp3", ".wav", ".flac", ".aac" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = $"File type {extension} is not supported. Allowed: {string.Join(", ", allowedExtensions)}" });
        }

        // Validate file size (50MB)
        if (file.Length > 50 * 1024 * 1024)
        {
            return BadRequest(new { message = "File size exceeds 50MB limit" });
        }

        // Read file into byte array
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var audioData = memoryStream.ToArray();

        var result = await _musicService.UploadTrackAudioAsync(id, audioData, file.FileName);
        if (!result)
        {
            return NotFound(new { message = "Track not found" });
        }

        return Ok(new { message = "Audio file uploaded successfully" });
    }

    // ==================== Helper Classes ====================

    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}
