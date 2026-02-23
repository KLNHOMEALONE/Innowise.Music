using Innowise.Music.Model;

namespace Innowise.Music.Services;

public interface INewsService
{
    Task<List<News>> GetNewsAsync();
}