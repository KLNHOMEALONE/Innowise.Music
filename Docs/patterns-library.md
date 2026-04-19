# .NET Music Streaming - Reusable Patterns Library

> Extracted from Innowise.Music project. Use as reference for new .NET MAUI + ASP.NET Core projects.

---

## 1. MVVM + CommunityToolkit Pattern

### ViewModel Base Pattern

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class ExampleViewModel : ObservableObject
{
    private readonly IExampleService _service;

    [ObservableProperty]
    private string _displayName;

    [ObservableProperty]
    private bool _isLoading;

    public ExampleViewModel(IExampleService service)
    {
        _service = service;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var data = await _service.GetDataAsync();
            DisplayName = data.Name;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

### DI Registration (MauiProgram.cs)

```csharp
// Services
builder.Services.AddSingleton<IExampleService, ExampleService>();
builder.Services.AddSingleton<ExampleViewModel>();
builder.Services.AddSingleton<ExamplePage>();

// HttpClient (singleton, shared)
builder.Services.AddSingleton(sp =>
{
    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
#if DEBUG
            return true; // Accept self-signed in dev
#else
            return errors == System.Net.Security.SslPolicyErrors.None;
#endif
        }
    };
    return new HttpClient(handler) { BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]) };
});
```

---

## 2. Validation Framework

### ValidatableObject<T>

```csharp
// Validations/ValidatableObject.cs
public class ValidatableObject<T> : ObservableObject, IValidity
{
    private IEnumerable<string> _errors;
    private bool _isValid;
    private T _value;

    public List<IValidationRule<T>> Validations { get; } = new();

    public IEnumerable<string> Errors
    {
        get => _errors;
        private set => SetProperty(ref _errors, value);
    }

    public bool IsValid
    {
        get => _isValid;
        private set => SetProperty(ref _isValid, value);
    }

    public T Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public ValidatableObject()
    {
        _isValid = true;
        _errors = Enumerable.Empty<string>();
    }

    public bool Validate()
    {
        Errors = Validations
            ?.Where(v => !v.Check(Value))
            ?.Select(v => v.ValidationMessage)
            ?.ToArray()
            ?? Enumerable.Empty<string>();
        IsValid = !Errors.Any();
        return IsValid;
    }
}
```

### IValidationRule<T>

```csharp
// Validations/IValidationRule.cs
public interface IValidationRule<T>
{
    string ValidationMessage { get; set; }
    bool Check(T value);
}
```

### Built-in Rules

```csharp
// IsNotNullOrEmptyRule
public class IsNotNullOrEmptyRule<T> : IValidationRule<T>
{
    public string ValidationMessage { get; set; }
    public bool Check(T value) => value is string str && !string.IsNullOrWhiteSpace(str);
}

// EmailRule
public class EmailRule<T> : IValidationRule<T>
{
    private readonly Regex _regex = new(@"^([\w.-]+)@([\w-]+)((\.(\w){2,3})+)$");
    public string ValidationMessage { get; set; }
    public bool Check(T value) => value is string str && _regex.IsMatch(str);
}

// CompareRule (password match)
public class CompareRule<T> : IValidationRule<T>
{
    public T CompareTo { get; set; }
    public string ValidationMessage { get; set; }
    public bool Check(T value) => EqualityComparer<T>.Default.Equals(value, CompareTo);
}
```

### Usage in ViewModel

```csharp
public partial class LoginViewModel : ObservableObject
{
    public ValidatableObject<string> Email { get; } = new();
    public ValidatableObject<string> Password { get; } = new();

    public LoginViewModel()
    {
        Email.Validations.Add(new IsNotNullOrEmptyRule<string>
        { ValidationMessage = "Email is required." });
        Email.Validations.Add(new EmailRule<string>
        { ValidationMessage = "Invalid email format." });

        Password.Validations.Add(new IsNotNullOrEmptyRule<string>
        { ValidationMessage = "Password is required." });
    }

    [RelayCommand]
    private void ValidateEmail() => Email.Validate();

    [RelayCommand]
    private void ValidatePassword() => Password.Validate();

    private bool ValidateForm()
        => Email.Validate() && Password.Validate();
}
```

### XAML Binding with Real-time Validation

```xml
<Entry Text="{Binding Email.Value, Mode=TwoWay}">
    <Entry.Behaviors>
        <mct:EventToCommandBehavior
            EventName="TextChanged"
            Command="{Binding ValidateEmailCommand}" />
    </Entry.Behaviors>
    <Entry.Triggers>
        <DataTrigger TargetType="Entry"
                     Binding="{Binding Email.IsValid}"
                     Value="False">
            <Setter Property="BackgroundColor" Value="#FFEBEE" />
        </DataTrigger>
    </Entry.Triggers>
</Entry>
<Label Text="{Binding Email.Errors, Converter={StaticResource FirstValidationErrorConverter}}"
       TextColor="Red"
       FontSize="12" />
```

---

## 3. JWT Authentication Pattern

### Token Storage & Refresh (MAUI Client)

```csharp
public class AuthenticationService : IAuthenticationService
{
    private const string AuthTokenKey = "auth_token";
    private const string RefreshTokenKey = "refresh_token";

    public async Task<bool> LoginAsync(LoginUserDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/Authentication/login", dto);
        if (!response.IsSuccessStatusCode) return false;

        var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
        await SecureStorage.Default.SetAsync(AuthTokenKey, authResponse.Token);
        await SecureStorage.Default.SetAsync(RefreshTokenKey, authResponse.RefreshToken);
        return true;
    }

    public async Task<string?> GetTokenAsync()
    {
        var token = await SecureStorage.Default.GetAsync(AuthTokenKey);
        if (string.IsNullOrEmpty(token)) return null;

        // Check expiry (with 1-minute buffer)
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        if (jwt.ValidTo < DateTime.UtcNow.AddMinutes(1))
        {
            // Auto-refresh
            var refreshed = await RefreshTokenAsync();
            return refreshed ? await SecureStorage.Default.GetAsync(AuthTokenKey) : null;
        }
        return token;
    }

    private async Task<bool> RefreshTokenAsync()
    {
        var token = await SecureStorage.Default.GetAsync(AuthTokenKey);
        var refreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
            return false;

        var response = await _httpClient.PostAsJsonAsync("/api/Authentication/refresh",
            new { Token = token, RefreshToken = refreshToken });

        if (!response.IsSuccessStatusCode)
        {
            await LogoutAsync();
            return false;
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
        await SecureStorage.Default.SetAsync(AuthTokenKey, authResponse.Token);
        await SecureStorage.Default.SetAsync(RefreshTokenKey, authResponse.RefreshToken);
        return true;
    }

    public async Task LogoutAsync()
    {
        SecureStorage.Remove(AuthTokenKey);
        SecureStorage.Remove(RefreshTokenKey);
        Preferences.Remove("user_name");
        await Shell.Current.GoToAsync("//Login");
    }
}
```

### Admin Dashboard Cookie + MemoryCache Pattern

```csharp
// Program.cs
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/";
    });

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
```

```csharp
// AuthService.cs (Admin)
public class AuthService : IAuthService
{
    private readonly IMemoryCache _cache;
    private readonly HttpContext _httpContext;

    public async Task<(bool Success, ClaimsPrincipal? Principal, string? Error)> LoginAndGetPrincipalAsync(string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/Authentication/login", new { Email = email, Password = password });
        if (!response.IsSuccessStatusCode)
            return (false, null, "Invalid credentials");

        var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();

        // Validate admin role
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(authResponse.Token);
        var claims = jwt.Claims.ToList();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));

        if (!principal.IsInRole("Administrator"))
            return (false, null, "Admin access required");

        // Cache tokens
        var userId = principal.FindFirst("uid")?.Value;
        _cache.Set($"AuthToken_{userId}", authResponse.Token, TimeSpan.FromHours(8));
        _cache.Set($"AuthRefreshToken_{userId}", authResponse.RefreshToken, TimeSpan.FromHours(8));

        // Sign in with cookie
        await _httpContext.SignInAsync(principal);
        return (true, principal, null);
    }

    public async Task<string?> GetTokenAsync()
    {
        var userId = _httpContext.User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(userId) || !_cache.TryGetValue($"AuthToken_{userId}", out string token))
        {
            // Attempt refresh
            if (_cache.TryGetValue($"AuthRefreshToken_{userId}", out string refreshToken))
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Authentication/refresh",
                    new { Token = token, RefreshToken = refreshToken });
                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
                    _cache.Set($"AuthToken_{userId}", authResponse.Token, TimeSpan.FromHours(8));
                    _cache.Set($"AuthRefreshToken_{userId}", authResponse.RefreshToken, TimeSpan.FromHours(8));
                    return authResponse.Token;
                }
            }
            return null;
        }
        return token;
    }
}
```

---

## 4. Service Layer Pattern

### Interface + Implementation

```csharp
// Services/IMusicService.cs
public interface IMusicService
{
    Task<(IEnumerable<Track> Tracks, int TotalCount)> SearchTracksAsync(string query, int skip, int take);
    Task<Track?> GetTrackAsync(Guid id);
    Task<Stream?> GetTrackAudioAsync(Guid trackId);
    Task<IEnumerable<Track>> GetRecommendedTracksAsync();
    Task<(bool IsFavorite, Track? Track)> ToggleFavoriteAsync(string userId, Guid trackId);
    Task RecordListeningHistoryAsync(string userId, Guid trackId);
    Task<IEnumerable<Track>> GetRecentTracksAsync(string userId, int count);
}

// Services/MusicService.cs
public class MusicService : IMusicService
{
    private readonly MusicIdentityDbContext _context;
    private readonly ILogger<MusicService> _logger;

    public MusicService(MusicIdentityDbContext context, ILogger<MusicService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IEnumerable<Track> Tracks, int TotalCount)> SearchTracksAsync(string query, int skip, int take)
    {
        var tracksQuery = _context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .Include(t => t.Genres)
            .Where(t => EF.Functions.ILike(t.Title, $"%{query}%") ||
                        EF.Functions.ILike(t.Artist.Name, $"%{query}%") ||
                        EF.Functions.ILike(t.Album.Title, $"%{query}%"));

        var totalCount = await tracksQuery.CountAsync();
        var tracks = await tracksQuery
            .OrderByDescending(t => t.PlayCount)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return (tracks, totalCount);
    }
}
```

---

## 5. API Design Patterns

### Standardized Response Models

```csharp
// Paginated response
public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

// Standard result
public class Result<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}
```

### Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MusicController : ControllerBase
{
    private readonly IMusicService _musicService;

    [HttpGet("tracks")]
    public async Task<ActionResult<PagedResponse<TrackDto>>> SearchTracks(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Min(pageSize, 50); // Cap page size
        var (tracks, totalCount) = await _musicService.SearchTracksAsync(query, (page - 1) * pageSize, pageSize);

        return Ok(new PagedResponse<TrackDto>
        {
            Items = _mapper.Map<IEnumerable<TrackDto>>(tracks),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }
}
```

---

## 6. Audio Streaming Pattern

### Range Request Support

```csharp
[HttpGet("tracks/{id}/stream")]
public async Task<IActionResult> StreamTrack(Guid id, [FromQuery] string? token)
{
    // Validate token or JWT
    if (!await ValidateStreamAccessAsync(id, token))
        return Unauthorized();

    var track = await _musicService.GetTrackAsync(id);
    if (track?.AudioData == null)
        return NotFound();

    // Support range requests
    var rangeHeader = Request.Headers.Range.ToString();
    if (!string.IsNullOrEmpty(rangeHeader) && rangeHeader.StartsWith("bytes="))
    {
        var range = RangeHeaderValue.Parse(rangeHeader);
        var start = range.Ranges.First().From ?? 0;
        var end = range.Ranges.First().To ?? track.AudioData.LongLength - 1;
        var length = end - start + 1;

        Response.Headers.AcceptRanges = "bytes";
        Response.StatusCode = 206; // Partial Content
        Response.Headers.ContentRange = $"bytes {start}-{end}/{track.AudioData.LongLength}";

        return File(track.AudioData.Skip((int)start).Take((int)length).ToArray(), GetContentType(track.AudioFormat));
    }

    Response.Headers.AcceptRanges = "bytes";
    return File(track.AudioData, GetContentType(track.AudioFormat));
}

private string GetContentType(string format) => format.ToUpper() switch
{
    "MP3" => "audio/mpeg",
    "WAV" => "audio/wav",
    "FLAC" => "audio/flac",
    "AAC" => "audio/aac",
    _ => "application/octet-stream"
};
```

---

## 7. Docker Compose Pattern

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: music_postgres
    environment:
      POSTGRES_USER: ${POSTGRES_USER:-admin}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-admin}
      POSTGRES_DB: musicidentity
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

  identity_server:
    build:
      context: .
      dockerfile: Innowise.MusicIdentityServer/Dockerfile
    container_name: music_identity_server
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__PostgresAppDbConnection=Server=postgres;Port=5432;Database=musicidentity;User Id=admin;Password=${POSTGRES_PASSWORD:-admin}
    ports:
      - "5236:8080"
      - "7008:8081"
    depends_on:
      - postgres

  admin_dashboard:
    build:
      context: .
      dockerfile: Innowise.Music.Admin/Dockerfile
    container_name: music_admin_dashboard
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ApiSettings__BaseUrl=http://music_identity_server:8080/api
    ports:
      - "5237:8080"
    depends_on:
      - identity_server

volumes:
  postgres_data:
```

---

## 8. Naming & Coding Conventions

| Convention | Example |
|------------|---------|
| PascalCase classes/methods | `MusicService`, `GetTrackAsync` |
| `_camelCase` private fields | `_musicService`, `_logger` |
| `I` prefix interfaces | `IMusicService`, `IAuthService` |
| `Async` suffix for async methods | `GetTrackAsync`, `LoginAsync` |
| `Dto` suffix for DTOs | `TrackDto`, `AuthenticationResponse` |
| `Controller` suffix for controllers | `MusicController`, `AuthController` |
| `Page` suffix for MAUI pages | `LoginPage`, `HomePage` |
| `ViewModel` suffix for ViewModels | `LoginViewModel`, `HomePageViewModel` |

---

## 9. Key NuGet Packages (Reference Versions)

```xml
<!-- MAUI Client -->
<PackageReference Include="CommunityToolkit.Maui" Version="9.0.0-preview4" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.16.0" />
<PackageReference Include="Google.Apis.Auth" Version="1.73.0" />

<!-- Identity Server -->
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.13" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.4" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.13" />
<PackageReference Include="Serilog.AspNetCore" Version="10.0.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />

<!-- Admin Dashboard -->
<PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="9.0.0" />
<PackageReference Include="TagLibSharp" Version="2.3.0" />
```

---

## 10. Database Indexing Pattern

```csharp
// In DbContext OnModelCreating:

// Full-text search indexes (GIN with trigram)
builder.Entity<Artist>().HasIndex(a => a.Name)
    .HasMethod("gin")
    .IsTsVectorExpressionIndex("english");

builder.Entity<Track>().HasIndex(t => t.Title)
    .HasMethod("gin")
    .IsTsVectorExpressionIndex("english");

// Performance indexes
builder.Entity<Track>().HasIndex(t => t.ArtistId);
builder.Entity<Track>().HasIndex(t => t.AlbumId);
builder.Entity<Track>().HasIndex(t => t.PlayCount).IsDescending();

builder.Entity<UserRecentTrack>().HasIndex(u => u.UserId);
builder.Entity<UserRecentTrack>().HasIndex(u => new { u.UserId, u.PlayedAt });

builder.Entity<UserFavoriteTrack>().HasIndex(u => u.UserId);
builder.Entity<UserFavoriteTrack>().HasIndex(u => new { u.UserId, u.TrackId }).IsUnique();
```

---

*This patterns library is extracted from the Innowise.Music project and is designed to be portable across .NET MAUI + ASP.NET Core projects.*
