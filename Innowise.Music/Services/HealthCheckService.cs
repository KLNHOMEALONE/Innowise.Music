/*
 * @file: HealthCheckService.cs
 * @description: Implementation of IHealthCheckService.
 * @dependencies: IHealthCheckService, IHttpHelper, IOptions<ApiSettings>
 * @created: 2026-04-22
 */
using Innowise.Music.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Innowise.Music.Services;

public class HealthCheckService : IHealthCheckService
{
    private readonly ApiSettings _apiSettings;
    private readonly IHttpHelper _httpHelper;

    public HealthCheckService(IHttpHelper httpHelper, IOptions<ApiSettings> apiSettings)
    {
        _httpHelper = httpHelper;
        _apiSettings = apiSettings.Value;
    }

    public async Task<bool> CheckIdentityServerHealthAsync()
    {
        try
        {
            var baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? _apiSettings.AndroidBaseUrl
                : _apiSettings.BaseUrl;

            // Just check the base URL - if the server responds at all, it's alive.
            var healthUrl = baseUrl.EndsWith("/") ? baseUrl : $"{baseUrl}/";

            System.Diagnostics.Debug.WriteLine($"[HealthCheck] Checking IdentityServer reachability at: {healthUrl}");
            
            // Create a fresh HttpClient for health checks to bypass any connection caching
            using var handler = _httpHelper.GetInsecureHandler();
            using var httpClient = new HttpClient(handler);
            
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            
            using var request = new HttpRequestMessage(HttpMethod.Get, healthUrl);
            request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            
            // SendAsync will throw if the server is totally unreachable (e.g. refused connection)
            var response = await httpClient.SendAsync(request, cts.Token);
            
            System.Diagnostics.Debug.WriteLine($"[HealthCheck] IdentityServer responded with: {response.StatusCode}");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HealthCheck] IdentityServer is unreachable: {ex.GetType().Name} - {ex.Message}");
            return false;
        }
    }
}
