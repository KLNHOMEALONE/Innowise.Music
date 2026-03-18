/*
 * @file: GoogleAuthService.cs
 * @description: Implementation of the Google authentication service.
 * @dependencies: IGoogleAuthService
 * @created: 2026-03-18
 */
using System;
using System.Threading.Tasks;
using System.Web;
using Innowise.Music.Configuration;
using Innowise.Music.View;
using Microsoft.Extensions.Options;
using Microsoft.Maui.Controls;

namespace Innowise.Music.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly GoogleAuthenticationSettings _googleAuthSettings;

        public GoogleAuthService(IOptions<GoogleAuthenticationSettings> googleAuthSettings)
        {
            _googleAuthSettings = googleAuthSettings.Value;
        }

        public async Task<string?> AcquireTokenAsync()
        {
            try
            {
                if (_googleAuthSettings.Google == null || string.IsNullOrEmpty(_googleAuthSettings.Google.ClientId))
                {
                    return null;
                }
                
                var clientId = _googleAuthSettings.Google.ClientId;
                var redirectUri = "myapp://oauth2redirect";
                var scope = "https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/userinfo.profile";
                var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope={scope}";
                
                await Shell.Current.GoToAsync($"{nameof(WebPage)}?url={HttpUtility.UrlEncode(authUrl)}");

                if (Shell.Current is not AppShell appShell)
                {
                    return null;
                }
                
                var authResult = await appShell.GetAuthResultAsync();

                if (string.IsNullOrEmpty(authResult))
                {
                    return null;
                }
                
                var uri = new Uri(authResult);
                var query = HttpUtility.ParseQueryString(uri.Query);
                var token = query.Get("code");
                
                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication failed: {ex.Message}");
                return null;
            }
        }
    }
}

