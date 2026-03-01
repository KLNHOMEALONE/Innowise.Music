namespace Innowise.Music.Services;

public class NavigationService : INavigationService
{
    public Task NavigateToAsync(string route)
    {
        return Shell.Current.GoToAsync(route, animate: true);
    }

    public Task NavigateToAsync(string route, IDictionary<string, object> parameters)
    {
        return Shell.Current.GoToAsync(route, animate: true, parameters);
    }

    public Task GoBackAsync()
    {
        return Shell.Current.GoToAsync("..", animate: true);
    }

    public Task GoBackAsync(IDictionary<string, object> parameters)
    {
        return Shell.Current.GoToAsync("..", animate: true, parameters);
    }

    public Task NavigateAndClearStackAsync(string route)
    {
        return Shell.Current.GoToAsync($"//{route}", animate: true);
    }
}
