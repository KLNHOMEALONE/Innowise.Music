using Innowise.Music.Admin.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Innowise.Music.Admin.Pages;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(returnUrl ?? "/");
        }

        ReturnUrl = returnUrl;
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? "/";
        
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var (success, claimsPrincipal, errorMessage) = await _authService.LoginAndGetPrincipalAsync(Input.Email, Input.Password);

            if (success && claimsPrincipal != null)
            {
                _logger.LogInformation("User {Email} logged in successfully.", Input.Email);
                
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    new AuthenticationProperties { IsPersistent = true });

                return LocalRedirect(ReturnUrl);
            }
            else
            {
                ErrorMessage = errorMessage ?? "Invalid email or password.";
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred during login for user {Email}", Input.Email);
            ErrorMessage = "An unexpected error occurred. Please try again.";
            return Page();
        }
    }
}
