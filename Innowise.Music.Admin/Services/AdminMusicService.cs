using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Innowise.Music.Admin.Models;

namespace Innowise.Music.Admin.Services;

public class AdminMusicService : IAdminMusicService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public AdminMusicService(HttpClient httpClient, IAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    private async Task AddAuthHeaderAsync()
    {
        var token = _authService.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    // ==================== Genre Operations ====================

    public async Task<List<Genre>> GetAllGenresAsync()
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.GetFromJsonAsync<List<Genre>>("api/admin/genres");
        return response ?? new List<Genre>();
    }

    public async Task<Genre?> GetGenreAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<Genre>($"api/admin/genres/{id}");
    }

    public async Task<Genre?> CreateGenreAsync(Genre genre)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/genres", genre);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Genre>();
        }
        return null;
    }

    public async Task<Genre?> UpdateGenreAsync(Guid id, Genre genre)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/genres/{id}", genre);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Genre>();
        }
        return null;
    }

    public async Task<bool> DeleteGenreAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/admin/genres/{id}");
        return response.IsSuccessStatusCode;
    }

    // ==================== Artist Operations ====================

    public async Task<PagedResponse<Artist>> GetAllArtistsAsync(int page = 1, int pageSize = 20)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.GetFromJsonAsync<PagedResponse<Artist>>(
            $"api/admin/artists?page={page}&pageSize={pageSize}");
        return response ?? new PagedResponse<Artist>();
    }

    public async Task<Artist?> GetArtistAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<Artist>($"api/admin/artists/{id}");
    }

    public async Task<Artist?> CreateArtistAsync(Artist artist)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/artists", artist);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Artist>();
        }
        return null;
    }

    public async Task<Artist?> UpdateArtistAsync(Guid id, Artist artist)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/artists/{id}", artist);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Artist>();
        }
        return null;
    }

    public async Task<bool> DeleteArtistAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/admin/artists/{id}");
        return response.IsSuccessStatusCode;
    }


    // ==================== Album Operations ====================

    public async Task<PagedResponse<Album>> GetAllAlbumsAsync(int page = 1, int pageSize = 20)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.GetFromJsonAsync<PagedResponse<Album>>(
            $"api/admin/albums?page={page}&pageSize={pageSize}");
        return response ?? new PagedResponse<Album>();
    }

    public async Task<Album?> GetAlbumAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<Album>($"api/admin/albums/{id}");
    }

    public async Task<Album?> CreateAlbumAsync(Album album)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/albums", album);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Album>();
        }
        return null;
    }

    public async Task<Album?> UpdateAlbumAsync(Guid id, Album album)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/albums/{id}", album);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Album>();
        }
        return null;
    }

    public async Task<bool> DeleteAlbumAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/admin/albums/{id}");
        return response.IsSuccessStatusCode;
    }


    // ==================== Track Operations ====================

    public async Task<PagedResponse<Track>> GetAllTracksAsync(int page = 1, int pageSize = 20)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.GetFromJsonAsync<PagedResponse<Track>>(
            $"api/admin/tracks?page={page}&pageSize={pageSize}");
        return response ?? new PagedResponse<Track>();
    }

    public async Task<Track?> GetTrackAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<Track>($"api/admin/tracks/{id}");
    }

    public async Task<Track?> CreateTrackAsync(Track track)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/tracks", track);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Track>();
        }
        return null;
    }

    public async Task<Track?> UpdateTrackAsync(Guid id, Track track)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/tracks/{id}", track);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Track>();
        }
        return null;
    }

    public async Task<bool> DeleteTrackAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/admin/tracks/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UploadTrackAudioAsync(Guid trackId, Stream stream, string fileName)
    {
        await AddAuthHeaderAsync();
        
        using var content = new MultipartFormDataContent();
        using var fileStream = new MemoryStream();
        await stream.CopyToAsync(fileStream);
        var fileContent = new ByteArrayContent(fileStream.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", fileName);

        var response = await _httpClient.PostAsync($"api/admin/tracks/{trackId}/upload", content);
        return response.IsSuccessStatusCode;
    }
    
    public async Task<BatchUploadResult> UploadTracksBatchAsync(IEnumerable<TrackUploadDto> tracks)
    {
        await AddAuthHeaderAsync();
        
        using var content = new MultipartFormDataContent();
        
        foreach (var track in tracks)
        {
            // Create a sub-content for each track with its metadata
            var trackContent = new ByteArrayContent(track.AudioData);
            trackContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(trackContent, "files", track.FileName);
        }
        
        var response = await _httpClient.PostAsync("api/admin/tracks/upload-batch", content);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<BatchUploadResult>()
                ?? new BatchUploadResult { Success = false, Errors = { "Unknown error occurred" } };
        }
        
        var errorContent = await response.Content.ReadAsStringAsync();
        return new BatchUploadResult
        {
            Success = false,
            Errors = { $"Upload failed: {response.ReasonPhrase}", errorContent }
        };
    }

}
