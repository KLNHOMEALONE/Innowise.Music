namespace Innowise.Music.Services;

public interface IHttpHelper
{
    HttpMessageHandler GetInsecureHandler();
}