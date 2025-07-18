// src/Web.Api/Controllers/AuthController.cs
using CleanEfApi.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication; // For ChallengeAsync, SignInManager.GetExternalLoginInfoAsync
using Microsoft.AspNetCore.Authentication.Google; // For GoogleDefaults.AuthenticationScheme (if not used directly elsewhere)

namespace CleanEfApi.Web.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    // --- Local Account Endpoints (Optional for Google-only scenario, but good to have) ---

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        var user = new IdentityUser { UserName = model.Email, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password!);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} registered successfully.", model.Email);
            return Ok(new AuthResponse { Success = true, Message = "User registered successfully." });
        }
        _logger.LogWarning("User registration failed for {Email}: {Errors}", model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
        return BadRequest(new AuthResponse { Success = false, Message = "Registration failed.", Errors = result.Errors.Select(e => e.Description).ToList() });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email!);
        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found for {Email}.", model.Email);
            return Unauthorized(new AuthResponse { Success = false, Message = "Invalid credentials." });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password!, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in successfully.", model.Email);
            var token = await GenerateJwtToken(user);
            return Ok(new AuthResponse { Success = true, Message = "Login successful.", Token = token });
        }
        _logger.LogWarning("Login failed for user {Email}.", model.Email);
        return Unauthorized(new AuthResponse { Success = false, Message = "Invalid credentials." });
    }

    // --- External Login Endpoints ---

    /// <summary>
    /// Initiates an external login challenge (e.g., Google, Microsoft).
    /// </summary>
    /// <param name="provider">The name of the external login provider (e.g., "Google").</param>
    /// <param name="returnUrl">The URL to return to after successful external login (typically your Scalar UI base URL).</param>
    /// <returns>A challenge that redirects to the external provider's login page.</returns>
    [HttpGet("external-login")]
    public IActionResult ExternalLogin([FromQuery] string provider, [FromQuery] string returnUrl)
    {
        // Construct the URL where the external provider will redirect *back to your API*
        // This must match one of the "Authorized redirect URIs" in your Google Cloud Console project.
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl }, Request.Scheme);

        // Configure properties for the external authentication challenge
        // The provider name (e.g., "Google") must match what's configured in Program.cs
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        _logger.LogInformation("Initiating external login challenge for provider: {Provider}", provider);
        return Challenge(properties, provider);
    }

    /// <summary>
    /// Handles the callback from the external login provider.
    /// This is where the user returns to your API after authenticating with the external provider.
    /// It then creates/links a local Identity user and redirects back to the client UI with a JWT.
    /// </summary>
    /// <param name="returnUrl">The original return URL for the client UI.</param>
    /// <param name="remoteError">Any error reported by the external provider.</param>
    /// <returns>A redirect to the client UI with the JWT in the URL fragment.</returns>
    [HttpGet("external-login-callback")]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/", string? remoteError = null)
    {
        if (remoteError != null)
        {
            _logger.LogError("External login error from provider: {Error}", remoteError);
            return Redirect($"{returnUrl}?error={Uri.EscapeDataString($"Error from external provider: {remoteError}")}");
        }

        // Get external login info from the provider (contains user's external ID and claims)
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogWarning("No external login info found after callback.");
            return Redirect($"{returnUrl}?error={Uri.EscapeDataString("Error loading external login information.")}");
        }

        // Try to sign in the user with this external login provider if they already have an account linked.
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in with {LoginProvider} account (existing user).", info.LoginProvider);
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            var token = await GenerateJwtToken(user!);
            // Redirect back to Scalar UI with token in fragment
            return Redirect($"{returnUrl}#access_token={token}&token_type=Bearer&expires_in={_configuration["Jwt:ExpireDays"]}");
        }
        if (result.IsLockedOut)
        {
            _logger.LogWarning("User locked out for {LoginProvider} account.", info.LoginProvider);
            return Redirect($"{returnUrl}?error={Uri.EscapeDataString("User account locked out.")}");
        }
        if (result.IsNotAllowed) // NEW: Handle NotAllowed status
        {
            _logger.LogWarning("Auth: User not allowed to login for {LoginProvider} account (e.g., email not confirmed).", info.LoginProvider);
            // Redirect to a page that explains account not confirmed or similar
            return Redirect($"{returnUrl}?error={Uri.EscapeDataString("Account not allowed to login. Please confirm your email or contact support.")}");
        }
        if (result.RequiresTwoFactor) // NEW: Handle 2FA required
        {
            _logger.LogInformation("Auth: User requires 2FA for {LoginProvider} account.", info.LoginProvider);
            // This is a complex flow. You'd typically redirect to a specific 2FA verification page.
            // For API, you might return a specific status or error indicating 2FA is needed.
            // Simplified redirect for demo:
            return Redirect($"{returnUrl}?error={Uri.EscapeDataString("Two-Factor Authentication is required. Please complete 2FA.")}");
        }
        // If none of the above, it means IsNotFound (or IsPartial, which is less common here)
        else
        {
            _logger.LogInformation("Auth: User not found with {LoginProvider}, attempting to create new account.", info.LoginProvider);
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Auth: External login callback failed: no email claim found from {LoginProvider}.", info.LoginProvider);
                return Redirect($"{returnUrl}?error={Uri.EscapeDataString("Error: Could not retrieve email from external provider.")}");
            }

            var newUser = new IdentityUser { UserName = email, Email = email };
            var createResult = await _userManager.CreateAsync(newUser);
            if (createResult.Succeeded)
            {
                createResult = await _userManager.AddLoginAsync(newUser, info);
                if (createResult.Succeeded)
                {
                    await _signInManager.SignInAsync(newUser, isPersistent: false);
                    _logger.LogInformation("Auth: New user account created and logged in with {LoginProvider} for {Email}.", info.LoginProvider, email);
                    var token = await GenerateJwtToken(newUser);
                    return Redirect($"{returnUrl}#access_token={token}&token_type=Bearer&expires_in={_configuration["Jwt:ExpireDays"]}");
                }
            }
            _logger.LogWarning("Auth: Error creating user account for {Email}: {Errors}", email, string.Join(", ", createResult.Errors.Select(e => e.Description)));
            return Redirect($"{returnUrl}?error={Uri.EscapeDataString("Error creating user account for external login.")}");
        }
    }

    /// <summary>
    /// This endpoint is used by Scalar's OpenAPI configuration (TokenUrl) to exchange
    /// a valid external authentication code (from the implicit flow or direct call) for a JWT.
    /// This is simplified for the specific Scalar integration.
    /// </summary>
    /// <returns>A JSON response containing the JWT.</returns>
    [HttpPost("external-login-token-exchange")]
    public async Task<IActionResult> ExternalLoginTokenExchange()
    {
        // This endpoint will be hit by Scalar/Swagger UI's internal mechanism.
        // It's a bit tricky because the browser has already completed the OAuth flow.
        // The most reliable way is often to use the cookie Identity sets after callback.
        // Or, for a truly direct token exchange, you'd need the auth code here (complex).

        // For simplicity and Scalar integration, we rely on the user having *just* authenticated
        // and having an Identity cookie. We then try to get the current user.
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync(); // Or any other way to get current authenticated user
        if (user == null)
        {
            user = await _signInManager.GetExternalAuthenticationSchemesAsync()
                          .ContinueWith(t => t.Result.FirstOrDefault(s => s.DisplayName == "Google")) // Example to get Google scheme
                          .ContinueWith(t => _signInManager.GetExternalLoginInfoAsync().Result) // Get login info from external cookie
                          .ContinueWith(t => _userManager.FindByLoginAsync(t.Result!.LoginProvider, t.Result.ProviderKey).Result); // Find user by external login

            if (user == null && User.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User); // Fallback to current authenticated user from claims
            }
        }

        if (user == null)
        {
            _logger.LogError("ExternalLoginTokenExchange: No authenticated user found to issue JWT.");
            return Unauthorized(new AuthResponse { Success = false, Message = "No active authentication session found to issue token." });
        }

        var token = await GenerateJwtToken(user);
        return Ok(new { access_token = token, token_type = "Bearer", expires_in = _configuration["Jwt:ExpireDays"] });
    }


    // --- JWT Generation Helper ---
    private async Task<string> GenerateJwtToken(IdentityUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id), // Subject (user ID)
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
            new Claim(ClaimTypes.NameIdentifier, user.Id), // Standard user ID claim
            new Claim(ClaimTypes.Name, user.UserName!), // Standard user name claim
            new Claim(ClaimTypes.Email, user.Email!) // Standard email claim
        };

        // Add user roles to claims
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}