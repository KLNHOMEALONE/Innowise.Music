using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Innowise.Music.Configuration;
using Innowise.Music.Model;
using Microsoft.Extensions.Options;

namespace Innowise.Music.Services;

public interface IFavoriteService
{
    Task<bool> ToggleFavoriteAsync(Guid trackId);
    Task<bool> IsFavoriteAsync(Guid trackId);
    Task<List<Track>> GetAllFavoritesAsync();
}

public class FavoriteService : IFavoriteService
{
    private readonly HttpHelper _httpHelper;
    private readonly IAuthenticationService _authenticationService;
    private readonly ApiSettings _apiSettings;

    public FavoriteService(
        HttpHelper httpHelper,
        IAuthenticationService authenticationService,
        IOptions<ApiSettings> apiSettings)
    {
        _httpHelper = httpHelper;
        _authenticationService = authenticationService;
        _apiSettings = apiSettings.Value;
    }

    private string GetApiUrl(string endpoint)
    {
        var baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? _apiSettings.AndroidBaseUrl
            : _apiSettings.BaseUrl;
        return $"{baseUrl}/api/Music/{endpoint}";
    }

    public async Task<bool> ToggleFavoriteAsync(Guid trackId)
    {
        try
        {
            var token = await _authenticationService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine($"[Favorite] No auth token, skipping toggle for track {trackId}");
                return false;
            }

            using var httpClient = new HttpClient(_httpHelper.GetInsecureHandler());
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = GetApiUrl($"tracks/{trackId}/favorite");
            System.Diagnostics.Debug.WriteLine($"[Favorite] Toggle: POST {url}");

            var response = await httpClient.PostAsync(url, null);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[Favorite] Toggle response: {content}");

                // Parse the JSON response { "isFavorite": true/false }
                if (TryParseIsFavorite(content, out var isFavorite))
                {
                    System.Diagnostics.Debug.WriteLine($"[Favorite] Track {trackId} isFavorite: {isFavorite}");
                    return isFavorite;
                }
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[Favorite] Failed to toggle for track {trackId}: {(int)response.StatusCode} {response.ReasonPhrase} - {content}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Favorite] Error toggling for track {trackId}: {ex.GetType().Name}: {ex.Message}");
        }

        return false;
    }

    public async Task<bool> IsFavoriteAsync(Guid trackId)
    {
        try
        {
            var token = await _authenticationService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine($"[Favorite] No auth token, skipping check for track {trackId}");
                return false;
            }

            using var httpClient = new HttpClient(_httpHelper.GetInsecureHandler());
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = GetApiUrl($"tracks/{trackId}/is-favorite");
            System.Diagnostics.Debug.WriteLine($"[Favorite] Check: GET {url}");

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (TryParseIsFavorite(content, out var isFavorite))
                {
                    System.Diagnostics.Debug.WriteLine($"[Favorite] Track {trackId} isFavorite: {isFavorite}");
                    return isFavorite;
                }
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[Favorite] Failed to check for track {trackId}: {(int)response.StatusCode} - {content}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Favorite] Error checking for track {trackId}: {ex.GetType().Name}: {ex.Message}");
        }

        return false;
    }

    public async Task<List<Track>> GetAllFavoritesAsync()
    {
        try
        {
            var token = await _authenticationService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine("[Favorite] No auth token, skipping get all favorites");
                return new List<Track>();
            }

            using var httpClient = new HttpClient(_httpHelper.GetInsecureHandler());
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = GetApiUrl("favorites");
            System.Diagnostics.Debug.WriteLine($"[Favorite] Fetching all favorites: GET {url}");

            var response = await httpClient.GetFromJsonAsync<FavoritesResponse>(url);
            System.Diagnostics.Debug.WriteLine($"[Favorite] Response tracks: {response?.Tracks?.Count ?? 0}");

            var streamBaseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? _apiSettings.AndroidStreamBaseUrl
                : _apiSettings.StreamBaseUrl;

            var tracks = response?.Tracks?.Select(t => new Track
            {
                Id = t.Id,
                Title = t.Title,
                ArtistName = t.Artist?.Name ?? "Unknown Artist",
                ImageUrl = t.Album?.CoverImageUrl ?? t.Artist?.ImageUrl ?? string.Empty,
                FileUri = $"{streamBaseUrl}/api/Music/tracks/{t.Id}/stream"
            }).ToList();

            return tracks ?? new List<Track>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Favorite] Error getting all favorites: {ex.GetType().Name}: {ex.Message}");
            return new List<Track>();
        }
    }

    private static bool TryParseIsFavorite(string json, out bool isFavorite)
    {
        isFavorite = false;
        if (string.IsNullOrEmpty(json))
            return false;

        // Simple parse of { "isFavorite": true/false }
        var key = "\"isFavorite\"";
        var index = json.IndexOf(key, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            var valueStart = json.IndexOf(':', index) + 1;
            if (valueStart > 0 && valueStart < json.Length)
            {
                var valueStr = json.Substring(valueStart).Trim().TrimStart('"');
                if (valueStr.StartsWith("true", StringComparison.OrdinalIgnoreCase))
                {
                    isFavorite = true;
                    return true;
                }
                if (valueStr.StartsWith("false", StringComparison.OrdinalIgnoreCase))
                {
                    isFavorite = false;
                    return true;
                }
            }
        }
        return false;
    }

    private class FavoritesResponse
    {
        public List<TrackDto> Tracks { get; set; } = new();
    }

    private class TrackDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public ArtistDto? Artist { get; set; }
        public AlbumDto? Album { get; set; }
    }

    private class ArtistDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    private class AlbumDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
    }
}
