using Innowise.Music.Admin.Components;
using Innowise.Music.Admin.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

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

// Register authentication service
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminMusicService, AdminMusicService>();
builder.Services.AddHttpContextAccessor();

// Add session support for token storage
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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
app.UseSession();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
