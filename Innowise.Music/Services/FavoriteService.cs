using System.Net.Http;
using System.Net.Http.Headers;
using Innowise.Music.Configuration;
using Microsoft.Extensions.Options;

namespace Innowise.Music.Services;

public interface IFavoriteService
{
    Task<bool> ToggleFavoriteAsync(Guid trackId);
    Task<bool> IsFavoriteAsync(Guid trackId);
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
}
