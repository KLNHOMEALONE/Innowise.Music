namespace Innowise.Music.Services;

public interface IHttpHelper
{
    HttpClientHandler GetInsecureHandler();
}