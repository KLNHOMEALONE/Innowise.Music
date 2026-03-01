using System.Net.Http.Json;
using Innowise.Music.Model;

namespace Innowise.Music.Services;

public class WebNewsService : INewsService
{
    private List<News> _news = new();
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authenticationService;

    public WebNewsService(HttpHelper httpHelper, IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        var handler = httpHelper.GetInsecureHandler();
        _httpClient = new HttpClient(handler);
    }


    public async Task<List<News>> GetNewsAsync()
    {
        var url = DeviceInfo.Platform == DevicePlatform.Android ? "https://10.0.2.2:7008/getnews" : "https://localhost:7008/getnews";
        
        var token = await _authenticationService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        var response = await _httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            _news = await response.Content.ReadFromJsonAsync<List<News>>();

        }
        return _news;
    }

}

