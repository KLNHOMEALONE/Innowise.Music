using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Innowise.Music.Configuration;
using Innowise.Music.Model;
using Microsoft.Extensions.Options;

namespace Innowise.Music.Services;

public class SearchService : ISearchService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authenticationService;
    private readonly ApiSettings _apiSettings;

    public SearchService(
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

    public async Task<UnifiedSearchResponse?> UnifiedSearchAsync(string query, int page = 1, int pageSize = 8)
    {
        try
        {
            
            var token = await _authenticationService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine("[Search] No auth token available");
                return null;
            }

            var url = $"{GetApiUrl("search")}?query={Uri.EscapeDataString(query)}&page={page}&pageSize={pageSize}";
            System.Diagnostics.Debug.WriteLine($"[Search] Calling: {url}");

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UnifiedSearchResponse>();
                System.Diagnostics.Debug.WriteLine($"[Search] Response: Page:{result?.Page}, T:{result?.Tracks?.Count}, Art:{result?.Artists?.Count}, Alb:{result?.Albums?.Count}");
                return result;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Search] Error: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }
}
