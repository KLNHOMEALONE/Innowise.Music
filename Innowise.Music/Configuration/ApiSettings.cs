namespace Innowise.Music.Configuration;

public class ApiSettings
{
    public const string SectionName = "ApiSettings";
    
    public string BaseUrl { get; set; } = string.Empty;
    public string AndroidBaseUrl { get; set; } = string.Empty;
}