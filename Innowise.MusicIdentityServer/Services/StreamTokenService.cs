using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Innowise.MusicIdentityServer.Services;

public interface IStreamTokenService
{
    string GenerateStreamToken(Guid trackId, string userId);
    bool ValidateStreamToken(string token, out Guid trackId);
}

public class StreamTokenService : IStreamTokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private const int TokenLifetimeMinutes = 5;

    public StreamTokenService(IConfiguration configuration)
    {
        _secretKey = configuration["JwtSettings:Key"]!;
        _issuer = configuration["JwtSettings:Issuer"]!;
        _audience = configuration["JwtSettings:Audience"]!;
    }

    public string GenerateStreamToken(Guid trackId, string userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("track_id", trackId.ToString()),
            new Claim("user_id", userId),
            new Claim(ClaimTypes.Role, "stream")
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(TokenLifetimeMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateStreamToken(string token, out Guid trackId)
    {
        trackId = Guid.Empty;

        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                IssuerSigningKey = key,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var jwt = (JwtSecurityToken)validatedToken;
            var trackIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "track_id");

            if (trackIdClaim != null && Guid.TryParse(trackIdClaim.Value, out trackId))
            {
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }
}
