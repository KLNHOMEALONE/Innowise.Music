using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Innowise.Music.Configuration;
using Innowise.Music.Model;
using Microsoft.Extensions.Options;

namespace Innowise.Music.Services;

public class RecommendationService : IRecommendationService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authenticationService;
    private readonly ApiSettings _apiSettings;

    public RecommendationService(
        HttpClient httpClient,
        IAuthenticationService authenticationService,
        IOptions<ApiSettings> apiSettings)
    {
        _httpClient = httpClient;
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

    public async Task<List<Track>> GetRecommendationsAsync()
    {
        try
        {
            var token = await _authenticationService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine("[Recommendations] No auth token available");
                return new List<Track>();
            }

            var url = GetApiUrl("recommendations");
            System.Diagnostics.Debug.WriteLine($"[Recommendations] Calling: {url}");

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[Recommendations] Failed: {response.StatusCode}");
                return new List<Track>();
            }

            var recommendationsResponse = await response.Content.ReadFromJsonAsync<TracksResponse>();
            System.Diagnostics.Debug.WriteLine($"[Recommendations] Response tracks: {recommendationsResponse?.Tracks?.Count ?? 0}");

            // Use HTTP for stream URLs (MediaElement can't handle self-signed HTTPS certs)
            var streamBaseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? _apiSettings.AndroidStreamBaseUrl
                : _apiSettings.StreamBaseUrl;

            var tracks = recommendationsResponse?.Tracks?.Select(t => new Track
            {
                Id = t.Id,
                Title = t.Title,
                ArtistName = t.Artist?.Name ?? "Unknown Artist",
                ImageUrl = t.Album?.CoverImageUrl ?? t.Artist?.ImageUrl ?? string.Empty,
                FileUri = $"{streamBaseUrl}/api/Music/tracks/{t.Id}/stream"
            }).ToList();

            if (tracks != null && tracks.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[Recommendations] Loaded {tracks.Count} tracks for streaming");
            }

            return tracks ?? new List<Track>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Recommendations] Error: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"[Recommendations] Inner: {ex.InnerException.Message}");
            }
            return new List<Track>();
        }
    }

    public async Task<List<Artist>> GetRecommendedArtistsAsync()
    {
        try
        {
            var token = await _authenticationService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine("[RecommendedArtists] No auth token available");
                return new List<Artist>();
            }

            var url = GetApiUrl("recommendations/artists");
            System.Diagnostics.Debug.WriteLine($"[RecommendedArtists] Calling: {url}");

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[RecommendedArtists] Failed: {response.StatusCode}");
                return new List<Artist>();
            }

            var artists = await response.Content.ReadFromJsonAsync<List<Artist>>();
            System.Diagnostics.Debug.WriteLine($"[RecommendedArtists] Response artists: {artists?.Count ?? 0}");

            return artists ?? new List<Artist>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecommendedArtists] Error: {ex.GetType().Name}: {ex.Message}");
            return new List<Artist>();
        }
    }
}
