using Innowise.Music.Admin.Auth;
using Innowise.Music.Admin.Components;
using Innowise.Music.Admin.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add Blazored.LocalStorage for token persistence
builder.Services.AddBlazoredLocalStorage();

// Configure API settings
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://music_identity_server:8080/api";
builder.Services.AddScoped(sp =>
{
    var handler = new SocketsHttpHandler
    {
        SslOptions = new SslClientAuthenticationOptions
        {
            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
        }
    };
    var httpClient = new HttpClient(handler) { BaseAddress = new Uri(apiBaseUrl) };
    return httpClient;
});

// Register AuthenticationStateProvider (dual registration pattern)
builder.Services.AddScoped<ApiAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(p =>
    p.GetRequiredService<ApiAuthenticationStateProvider>());

// Register authentication and music services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminMusicService, AdminMusicService>();
builder.Services.AddScoped<IMetadataExtractionService, MetadataExtractionService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
