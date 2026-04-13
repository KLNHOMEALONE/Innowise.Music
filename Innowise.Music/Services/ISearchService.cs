using System.Threading.Tasks;
using Innowise.Music.Model;

namespace Innowise.Music.Services;

public interface ISearchService
{
    Task<UnifiedSearchResponse?> UnifiedSearchAsync(string query);
}
