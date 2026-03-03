using AutoMapper;
using Innowise.MusicIdentityServer.Data;
using Innowise.MusicIdentityServer.Models.User;
using Innowise.MusicIdentityServer.Static;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Innowise.MusicIdentityServer.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;
    private readonly IMapper _mapper;
    private readonly UserManager<ApiUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthenticationController(ILogger<AuthenticationController> logger, IMapper mapper, UserManager<ApiUser> userManager, IConfiguration configuration)
    {
        this._logger = logger;
        this._mapper = mapper;
        this._userManager = userManager;
        this._configuration = configuration;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(UserDto userDto)
    {
        _logger.LogInformation($"Registration Attempt for {userDto.Email} ");
        try
        {
            var user = _mapper.Map<ApiUser>(userDto);
            user.UserName = userDto.Email;
            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (result.Succeeded == false)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            await _userManager.AddToRoleAsync(user, "User");
            return Accepted();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Something Went Wrong in the {nameof(Register)}");
            return Problem($"Something Went Wrong in the {nameof(Register)}", statusCode: 500);
        }
    }

    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<AuthenticationResponse>> Login(LoginUserDto userDto)
    {
        _logger.LogInformation($"Login Attempt for {userDto.Email} ");
        try
        {
            var user = await _userManager.FindByEmailAsync(userDto.Email);
            if (user == null) return Unauthorized(userDto);
            var passwordValid = await _userManager.CheckPasswordAsync(user, userDto.Password);

            if (passwordValid == false)
            {
                return Unauthorized(userDto);
            }

            string tokenString = await GenerateToken(user);
            string refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            // Configurable refresh token duration, defaulting to 7 days
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var response = new AuthenticationResponse
            {
                Email = userDto.Email,
                Token = tokenString,
                RefreshToken = refreshToken,
                UserId = user.Id,
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Something Went Wrong in the {nameof(Login)}");
            return Problem($"Something Went Wrong in the {nameof(Login)}", statusCode: 500);
        }
    }

    [HttpPost]
    [Route("refresh")]
    public async Task<ActionResult<AuthenticationResponse>> Refresh([FromBody] TokenRequestDto tokenRequestDto)
    {
        if (tokenRequestDto is null)
        {
            return BadRequest("Invalid client request");
        }

        string accessToken = tokenRequestDto.Token;
        string refreshToken = tokenRequestDto.RefreshToken;

        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
        {
            return BadRequest("Invalid access token or refresh token");
        }

        string email = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value
                       ?? principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("Invalid access token or refresh token");
        }

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return BadRequest("Invalid access token or refresh token");
        }

        var newAccessToken = await GenerateToken(user);
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        await _userManager.UpdateAsync(user);

        return new AuthenticationResponse
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            Email = user.Email,
            UserId = user.Id
        };
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"])),
            ValidateLifetime = false // we want to get payload even if it's expired
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        var jwtSecurityToken = securityToken as JwtSecurityToken;
        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> GenerateToken(ApiUser user)
    {
        var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
        var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

        var userClaims = await _userManager.GetClaimsAsync(user);

        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(CustomClaimTypes.Uid, user.Id)
            }
        .Union(userClaims)
        .Union(roleClaims);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(Convert.ToInt32(_configuration["JwtSettings:Duration"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}