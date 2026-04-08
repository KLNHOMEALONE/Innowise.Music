using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Innowise.Music.Configuration;
using Innowise.Music.Model;
using Microsoft.Extensions.Options;

namespace Innowise.Music.Services;

public interface IHistoryService
{
    Task RecordPlayAsync(Guid trackId);
    Task<List<Track>> GetRecentTracksAsync(int count = 5);
}

public class HistoryService : IHistoryService
{
    private readonly HttpHelper _httpHelper;
    private readonly IAuthenticationService _authenticationService;
    private readonly ApiSettings _apiSettings;

    public HistoryService(
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

    public async Task RecordPlayAsync(Guid trackId)
    {
        try
        {
            var token = await _authenticationService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine($"[History] No auth token, skipping history record for track {trackId}");
                return;
            }

            using var httpClient = new HttpClient(_httpHelper.GetInsecureHandler());
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = GetApiUrl($"tracks/{trackId}/history");
            System.Diagnostics.Debug.WriteLine($"[History] Recording play: POST {url}");

            var response = await httpClient.PostAsync(url, null);

            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[History] Recorded play for track {trackId}");
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[History] Failed to record play for track {trackId}: {(int)response.StatusCode} {response.ReasonPhrase} - {content}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[History] Error recording play for track {trackId}: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"[History] Inner: {ex.InnerException.Message}");
            }
        }
    }

    public async Task<List<Track>> GetRecentTracksAsync(int count = 5)
    {
        try
        {
            var token = await _authenticationService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine("[History] No auth token available");
                return new List<Track>();
            }

            using var httpClient = new HttpClient(_httpHelper.GetInsecureHandler());
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = GetApiUrl($"history/recent?count={count}");
            System.Diagnostics.Debug.WriteLine($"[History] Fetching recent: GET {url}");

            var response = await httpClient.GetFromJsonAsync<RecentTracksResponse>(url);
            System.Diagnostics.Debug.WriteLine($"[History] Response tracks: {response?.Tracks?.Count ?? 0}");

            var streamBaseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? "http://10.0.2.2:5236"
                : "http://localhost:5236";

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
            System.Diagnostics.Debug.WriteLine($"[History] Error: {ex.GetType().Name}: {ex.Message}");
            return new List<Track>();
        }
    }

    private class RecentTracksResponse
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
