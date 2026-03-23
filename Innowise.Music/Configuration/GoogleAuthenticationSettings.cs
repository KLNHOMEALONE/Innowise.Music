namespace Innowise.Music.Configuration;

public class GoogleAuthenticationSettings
{
    public const string SectionName = "GoogleAuthentication";

    public GoogleSettings? Google { get; set; }
}

public class GoogleSettings
{
    public string? ClientId { get; set; }
    public string? WindowsClientId { get; set; }
    public string? AndroidClientId { get; set; }
}
