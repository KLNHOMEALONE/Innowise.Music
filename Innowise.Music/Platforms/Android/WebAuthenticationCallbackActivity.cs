using Android.App;
using Android.Content;
using Android.Content.PM;

namespace Innowise.Music.Platforms.Android;

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[] { Intent.ActionView }, AutoVerify = true,
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = CALLBACK_SCHEME,
    DataPathPrefix = "/oauth2redirect")]
//[IntentFilter(new[] { Intent.ActionView }, AutoVerify = true,
//    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
//    DataScheme = CALLBACK_SCHEME,
//    DataHost = "oauth2redirect")]
public class WebAuthenticationCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
{
    const string CALLBACK_SCHEME = "com.klnhomealone.innomusic";
}