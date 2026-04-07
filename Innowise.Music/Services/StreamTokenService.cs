using System.Net.Http.Headers;
using System.Net.Http.Json;
using Innowise.Music.Configuration;
using Microsoft.Extensions.Options;

namespace Innowise.Music.Services;

public interface IStreamTokenService
{
    Task<string?> GetStreamTokenAsync(Guid trackId);
}

public class StreamTokenService : IStreamTokenService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authenticationService;
    private readonly ApiSettings _apiSettings;

    public StreamTokenService(
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

    public async Task<string?> GetStreamTokenAsync(Guid trackId)
    {
        try
        {
            var token = await _authenticationService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = GetApiUrl($"tracks/{trackId}/stream-token");
            var response = await _httpClient.GetFromJsonAsync<StreamTokenResponse>(url);
            return response?.Token;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StreamTokenService Error: {ex.Message}");
            return null;
        }
    }

    private class StreamTokenResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
