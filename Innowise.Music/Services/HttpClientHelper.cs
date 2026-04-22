namespace Innowise.Music.Services;

public class HttpHelper : IHttpHelper
{
    public HttpMessageHandler GetInsecureHandler()
    {
#if ANDROID
        var handler = new Xamarin.Android.Net.AndroidMessageHandler();
#if DEBUG
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            if (cert != null && cert.Issuer.Contains("localhost", StringComparison.OrdinalIgnoreCase))
                return true;
            return errors == System.Net.Security.SslPolicyErrors.None;
        };
#endif
        return handler;
#elif IOS
        var handler = new NSUrlSessionHandler();
#if DEBUG
        handler.TrustOverrideForUrl = (sender, url, trust) => url.Contains("localhost") || url.Contains("10.0.2.2");
#endif
        return handler;
#else
        HttpClientHandler handler = new HttpClientHandler();
#if DEBUG
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            if (cert != null && cert.Issuer.Contains("localhost", StringComparison.OrdinalIgnoreCase))
                return true;
            return errors == System.Net.Security.SslPolicyErrors.None;
        };
#endif
        return handler;
#endif
    }
}
