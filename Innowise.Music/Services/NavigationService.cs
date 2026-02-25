namespace Innowise.Music.Services;

public class NavigationService : INavigationService
{
    public Task NavigateToAsync(string route)
    {
        return Shell.Current.GoToAsync(route);
    }

    public Task NavigateToAsync(string route, IDictionary<string, object> parameters)
    {
        return Shell.Current.GoToAsync(route, parameters);
    }

    public Task GoBackAsync()
    {
        return Shell.Current.GoToAsync("..");
    }

    public Task GoBackAsync(IDictionary<string, object> parameters)
    {
        return Shell.Current.GoToAsync("..", parameters);
    }

    public Task NavigateAndClearStackAsync(string route)
    {
        return Shell.Current.GoToAsync($"//{route}");
    }
}
