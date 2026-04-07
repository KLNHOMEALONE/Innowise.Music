using Innowise.Music.Model;

namespace Innowise.Music.Services;

public interface IRecommendationService
{
    Task<List<Track>> GetRecommendationsAsync();
}
