# Blazor Server Admin Dashboard - Complete Template

> Reusable template extracted from Innowise.Music.Admin. Includes authentication, layout, CRUD patterns, and service integration.

---

## Project Structure

```
YourProject.Admin/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   ├── LoginLayout.razor
│   │   └── NavMenu.razor
│   ├── Pages/
│   │   ├── Dashboard.razor
│   │   └── [Entity]s/
│   │       ├── [Entity]List.razor
│   │       └── [Entity]Form.razor
│   ├── Shared/
│   │   ├── ConfirmDialog.razor
│   │   ├── LoadingSpinner.razor
│   │   └── RedirectToLogin.razor
│   ├── _Imports.razor
│   └── App.razor
├── Pages/
│   ├── _Host.cshtml
│   ├── _ViewImports.cshtml
│   ├── Login.cshtml
│   ├── Login.cshtml.cs
│   ├── Logout.cshtml
│   └── Logout.cshtml.cs
├── Services/
│   ├── IAuthService.cs
│   ├── AuthService.cs
│   ├── I[Entity]Service.cs
│   └── [Entity]Service.cs
├── Models/
│   ├── [Entity].cs
│   └── PagedResponse.cs
├── wwwroot/
│   └── css/
│       └── app.css
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

---

## Program.cs

```csharp
using YourProject.Admin.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Authentication - Cookie-based for Blazor Server session
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/";
    });

builder.Services.AddCascadingAuthenticationState();

// API Configuration
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? "http://localhost:8080/api/";

// Auth Service - HttpClient + MemoryCache for JWT tokens
builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Entity Service (repeat for each entity)
builder.Services.AddHttpClient<IEntityService, EntityService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Infrastructure
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Middleware Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
```

---

## appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ApiSettings": {
    "BaseUrl": "http://api_service:8080/api/"
  }
}
```

## appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ApiSettings": {
    "BaseUrl": "https://localhost:7008/api/"
  }
}
```

---

## Services

### IAuthService.cs

```csharp
using System.Security.Claims;

namespace YourProject.Admin.Services;

public interface IAuthService
{
    Task<(bool Success, ClaimsPrincipal? Principal, string? Error)> LoginAndGetPrincipalAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<bool> IsInRoleAsync(string role);
    Task<string?> GetTokenAsync();
}
```

### AuthService.cs

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace YourProject.Admin.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        AuthenticationStateProvider authStateProvider,
        IMemoryCache memoryCache,
        ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _authStateProvider = authStateProvider;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<(bool Success, ClaimsPrincipal? Principal, string? Error)> LoginAndGetPrincipalAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("Authentication/login", new { Email = email, Password = password });
            if (!response.IsSuccessStatusCode)
                return (false, null, "Invalid credentials");

            var content = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (string.IsNullOrWhiteSpace(authResponse?.Token))
                return (false, null, "No token received");

            var principal = CreateClaimsPrincipalFromToken(authResponse.Token);

            // Optional: Check role
            // if (!principal.IsInRole("Administrator"))
            //     return (false, null, "Access denied");

            // Cache tokens
            var userId = principal.FindFirst("uid")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _memoryCache.Set(GetTokenCacheKey(userId), authResponse.Token, TimeSpan.FromHours(8));
                _memoryCache.Set(GetRefreshTokenCacheKey(userId), authResponse.RefreshToken, TimeSpan.FromHours(8));
            }

            return (true, principal, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error");
            return (false, null, "An error occurred");
        }
    }

    public async Task LogoutAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userId = authState.User.FindFirstValue("uid");

        if (!string.IsNullOrEmpty(userId))
        {
            _memoryCache.Remove(GetTokenCacheKey(userId));
            _memoryCache.Remove(GetRefreshTokenCacheKey(userId));
        }

        if (_httpContextAccessor.HttpContext != null)
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public async Task<string?> GetTokenAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userId = authState.User.FindFirst("uid")?.Value;

        if (string.IsNullOrEmpty(userId))
            return null;

        if (_memoryCache.Get<string>(GetTokenCacheKey(userId)) is string token)
            return token;

        // Try refresh
        if (_memoryCache.Get<string>(GetRefreshTokenCacheKey(userId)) is string refreshToken)
            return await RefreshTokenAsync(userId, token, refreshToken);

        return null;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.Identity?.IsAuthenticated ?? false;
    }

    public async Task<bool> IsInRoleAsync(string role)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.IsInRole(role);
    }

    private async Task<string?> RefreshTokenAsync(string userId, string? accessToken, string refreshToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("Authentication/refresh", new { Token = accessToken, RefreshToken = refreshToken });
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (string.IsNullOrWhiteSpace(authResponse?.Token))
                return null;

            _memoryCache.Set(GetTokenCacheKey(userId), authResponse.Token, TimeSpan.FromHours(8));
            _memoryCache.Set(GetRefreshTokenCacheKey(userId), authResponse.RefreshToken, TimeSpan.FromHours(8));

            return authResponse.Token;
        }
        catch
        {
            return null;
        }
    }

    private ClaimsPrincipal CreateClaimsPrincipalFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var claims = jwt.Claims.ToList();

        if (claims.All(c => c.Type != ClaimTypes.Name))
            claims.Add(new Claim(ClaimTypes.Name, jwt.Subject));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
        return new ClaimsPrincipal(identity);
    }

    private string GetTokenCacheKey(string userId) => $"AuthToken_{userId}";
    private string GetRefreshTokenCacheKey(string userId) => $"AuthRefreshToken_{userId}";

    private class AuthResponse
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
```

### IEntityService.cs

```csharp
namespace YourProject.Admin.Services;

public interface IEntityService
{
    Task<List<Entity>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<Entity?> GetByIdAsync(Guid id);
    Task<Entity> CreateAsync(Entity entity);
    Task<Entity?> UpdateAsync(Guid id, Entity entity);
    Task<bool> DeleteAsync(Guid id);
}
```

### EntityService.cs

```csharp
namespace YourProject.Admin.Services;

public class EntityService : IEntityService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private readonly ILogger<EntityService> _logger;

    public EntityService(HttpClient httpClient, IAuthService authService, ILogger<EntityService> logger)
    {
        _httpClient = httpClient;
        _authService = authService;
        _logger = logger;
    }

    private async Task AddAuthHeaderAsync()
    {
        var token = await _authService.GetTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            !string.IsNullOrEmpty(token)
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
    }

    public async Task<List<Entity>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        await AddAuthHeaderAsync();
        try
        {
            return await _httpClient.GetFromJsonAsync<List<Entity>>($"api/entities?page={page}&pageSize={pageSize}") ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get entities");
            return [];
        }
    }

    public async Task<Entity?> GetByIdAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        try
        {
            return await _httpClient.GetFromJsonAsync<Entity>($"api/entities/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get entity {Id}", id);
            return null;
        }
    }

    public async Task<Entity> CreateAsync(Entity entity)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/entities", entity);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Entity>() ?? throw new Exception("Failed to create entity");
    }

    public async Task<Entity?> UpdateAsync(Guid id, Entity entity)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/entities/{id}", entity);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<Entity>()
            : null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/entities/{id}");
        return response.IsSuccessStatusCode;
    }
}

public class Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

---

## Pages

### Login.cshtml

```razor
@page "/login"
@model YourProject.Admin.Pages.LoginModel
@{
    ViewData["Title"] = "Login";
}

<div class="login-page">
    <div class="login-card">
        <h1>Admin Dashboard</h1>

        @if (!string.IsNullOrEmpty(Model.ErrorMessage))
        {
            <div class="error-message">@Model.ErrorMessage</div>
        }

        <form method="post">
            <div class="form-group">
                <label asp-for="Input.Email"></label>
                <input asp-for="Input.Email" class="form-control" required />
                <span asp-validation-for="Input.Email"></span>
            </div>

            <div class="form-group">
                <label asp-for="Input.Password"></label>
                <input asp-for="Input.Password" type="password" class="form-control" required />
                <span asp-validation-for="Input.Password"></span>
            </div>

            <button type="submit" class="btn-primary">Login</button>
        </form>
    </div>
</div>
```

### Login.cshtml.cs

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace YourProject.Admin.Pages;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return LocalRedirect(returnUrl ?? "/");

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return Page();

        var (success, principal, error) = await _authService.LoginAndGetPrincipalAsync(Input.Email, Input.Password);

        if (success && principal != null)
        {
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true });

            return LocalRedirect(returnUrl ?? "/");
        }

        ErrorMessage = error ?? "Invalid credentials";
        return Page();
    }
}
```

### Logout.cshtml.cs

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YourProject.Admin.Pages;

public class LogoutModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}
```

---

## Blazor Components

### App.razor

```razor
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(Layout.MainLayout)">
                <NotAuthorized>
                    @if (context.User?.Identity?.IsAuthenticated == false)
                    {
                        <RedirectToLogin />
                    }
                    else
                    {
                        <p>You are not authorized to access this resource.</p>
                    }
                </NotAuthorized>
                <Authorizing>
                    <p>Loading...</p>
                </Authorizing>
            </AuthorizeRouteView>
            <FocusOnNavigate RouteData="@routeData" Selector="h1" />
        </Found>
        <NotFound>
            <LayoutView Layout="@typeof(Layout.MainLayout)">
                <p>Page not found.</p>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```

### _Imports.razor

```razor
@using System.Net.Http
@using System.Linq
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.JSInterop
@using YourProject.Admin.Components
@using YourProject.Admin.Components.Layout
@using YourProject.Admin.Components.Pages
@using YourProject.Admin.Components.Shared
@using YourProject.Admin.Services
@using YourProject.Admin.Models
```

### MainLayout.razor

```razor
@inherits LayoutComponentBase
@inject NavigationManager Navigation
@inject IAuthService AuthService

<div class="admin-layout">
    <NavMenu />
    <main class="main-content">
        <header class="admin-header">
            <h1>Dashboard</h1>
            <div class="header-actions">
                <span>@_userName</span>
                <button class="btn-logout" @onclick="Logout">Logout</button>
            </div>
        </header>
        <div class="page-content">
            @Body
        </div>
    </main>
</div>

@code {
    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    private string _userName = "Admin";

    protected override async Task OnParametersSetAsync()
    {
        var authState = await AuthenticationStateTask;
        if (authState.User.Identity?.IsAuthenticated == true)
            _userName = authState.User.Identity.Name ?? "Admin";
    }

    private void Logout()
    {
        Navigation.NavigateTo("/logout", forceLoad: true);
    }
}
```

### RedirectToLogin.razor

```razor
@inject NavigationManager Navigation

@code {
    protected override void OnInitialized()
    {
        Navigation.NavigateTo("login", forceLoad: true);
    }
}
```

### EntityList.razor (CRUD List Page)

```razor
@page "/entities"
@attribute [Authorize]
@inject IEntityService Service

<h1>Entities</h1>

@if (_entities == null)
{
    <p>Loading...</p>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Name</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var entity in _entities)
            {
                <tr>
                    <td>@entity.Name</td>
                    <td>
                        <a href="/entities/@entity.Id/edit" class="btn">Edit</a>
                        <button class="btn btn-danger" @onclick="() => Delete(entity.Id)">Delete</button>
                    </td>
                </tr>
            }
        </tbody>
    </table>
    <a href="/entities/new" class="btn btn-primary">Add New</a>
}

@code {
    private List<Entity> _entities = [];

    protected override async Task OnInitializedAsync()
    {
        _entities = await Service.GetAllAsync();
    }

    private async Task Delete(Guid id)
    {
        if (await Service.DeleteAsync(id))
            _entities = _entities.Where(e => e.Id != id).ToList();
    }
}
```

### EntityForm.razor (CRUD Form Page)

```razor
@page "/entities/new"
@page "/entities/{id:guid}/edit"
@attribute [Authorize]
@inject IEntityService Service
@inject NavigationManager Navigation
@inject ILogger<EntityForm> Logger

<h1>@(_entity.Id == Guid.Empty ? "New Entity" : "Edit Entity")</h1>

<EditForm Model="_entity" OnValidSubmit="Save">
    <DataAnnotationsValidator />

    <div class="form-group">
        <label asp-for="_entity.Name"></label>
        <InputText @bind-Value="_entity.Name" class="form-control" />
        <ValidationMessage For="@(() => _entity.Name)" />
    </div>

    <button type="submit" class="btn btn-primary">Save</button>
    <a href="/entities" class="btn">Cancel</a>
</EditForm>

@code {
    [Parameter]
    public Guid Id { get; set; }

    private Entity _entity = new();

    protected override async Task OnInitializedAsync()
    {
        if (Id != Guid.Empty)
        {
            var fetched = await Service.GetByIdAsync(Id);
            if (fetched != null)
                _entity = fetched;
        }
    }

    private async Task Save()
    {
        try
        {
            if (_entity.Id == Guid.Empty)
                await Service.CreateAsync(_entity);
            else
                await Service.UpdateAsync(_entity.Id, _entity);

            Navigation.NavigateTo("/entities");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save entity");
        }
    }
}
```

---

## _Host.cshtml

```razor
@page "/"
@using Microsoft.AspNetCore.Components.Web
@namespace YourProject.Admin.Components
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Admin Dashboard</title>
    <base href="~/" />
    <link rel="stylesheet" href="css/app.css" />
    <link href="YourProject.Admin.styles.css" rel="stylesheet" />
</head>
<body>
    <component type="typeof(App)" render-mode="Server" />
    <script src="_framework/blazor.server.js"></script>
</body>
</html>
```

---

## Dockerfile

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["YourProject.Admin/YourProject.Admin.csproj", "YourProject.Admin/"]
RUN dotnet restore "YourProject.Admin/YourProject.Admin.csproj"
COPY . .
WORKDIR "/src/YourProject.Admin"
RUN dotnet build "YourProject.Admin.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "YourProject.Admin.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YourProject.Admin.dll"]
```

---

## docker-compose.yml (Add Service)

```yaml
admin_dashboard:
  build:
    context: .
    dockerfile: YourProject.Admin/Dockerfile
  container_name: admin_dashboard
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
    - ApiSettings__BaseUrl=http://api_service:8080/api/
  ports:
    - "5237:8080"
  depends_on:
    - api_service
```

---

## Key Patterns Summary

| Pattern | Implementation |
|---------|---------------|
| **Auth** | Cookie-based Blazor session + JWT for API calls |
| **Token Storage** | `IMemoryCache` with sliding expiration |
| **Token Refresh** | Automatic in `GetTokenAsync()` |
| **HTTP Auth** | `AddAuthHeaderAsync()` before each API call |
| **Route Protection** | `[Authorize]` + `AuthorizeRouteView` |
| **Login Redirect** | `forceLoad: true` to break Blazor circuit |
| **DI Lifetime** | Scoped HttpClients with `AddHttpClient` |
| **Error Handling** | Try-catch with logging, graceful fallbacks |
| **Configuration** | API URL in appsettings, environment overrides |

---

*Extracted from Innowise.Music.Admin. Use as template for new Blazor Server admin dashboards.*
