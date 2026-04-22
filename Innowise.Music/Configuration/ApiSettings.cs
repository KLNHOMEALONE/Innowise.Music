namespace Innowise.Music.Configuration;

public class ApiSettings
{
    public const string SectionName = "ApiSettings";
    
    public string BaseUrl { get; set; } = string.Empty;
    public string AndroidBaseUrl { get; set; } = string.Empty;
    
    public string StreamBaseUrl { get; set; } = string.Empty;
    public string AndroidStreamBaseUrl { get; set; } = string.Empty;
    
    public int SearchPageSize { get; set; } = 8;
}