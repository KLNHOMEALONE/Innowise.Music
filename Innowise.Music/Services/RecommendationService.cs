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
                return new List<Track>();
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = GetApiUrl("recommendations");
            var response = await _httpClient.GetFromJsonAsync<RecommendationsResponse>(url);

            // Use HTTP for stream URLs (MediaElement can't handle self-signed HTTPS certs)
            var streamBaseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? "http://10.0.2.2:5236"
                : "http://localhost:5236";

            return response?.Tracks?.Select(t => new Track
            {
                Id = t.Id,
                Title = t.Title,
                ArtistName = t.Artist?.Name ?? "Unknown Artist",
                ImageUrl = t.Album?.CoverImageUrl ?? t.Artist?.ImageUrl ?? string.Empty,
                FileUri = $"{streamBaseUrl}/api/Music/tracks/{t.Id}/stream"
            }).ToList() ?? new List<Track>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RecommendationService Error: {ex.Message}");
            return new List<Track>();
        }
    }

    private class RecommendationsResponse
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
