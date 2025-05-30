using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using DeafAssistant.Context;
using DeafAssistant.Models;
using DeafAssistant.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DeafAssistant.Controllers;

/// <summary>
/// Controller for managing user authentication and account operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;

    /// <summary>
    /// Constructor for AccountController
    /// </summary>
    /// <param name="userManager">ASP.NET Core Identity user manager</param>
    /// <param name="signInManager">ASP.NET Core Identity sign-in manager</param>
    /// <param name="roleManager">ASP.NET Core Identity role manager</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="webHostEnvironment">Web host environment for file operations</param>
    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="model">Registration data</param>
    /// <returns>Registration result</returns>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterModel model)
    {
        var response = new AuthResponse();

        if (await _userManager.FindByEmailAsync(model.Email) != null)
        {
            response.Errors.Add("User with this email already exists");
            return BadRequest(response);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            Birthday = model.Birthday,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true,
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Assign default User role if it exists
            if (await _roleManager.RoleExistsAsync("User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
                user.Role = "User";
            }

            // Create a default free subscription for the user
            await CreateDefaultFreeSubscription(user.Id);

            // Generate JWT token for the user
            var jwtToken = await GenerateJwtToken(user);
            response.Succeeded = true;
            response.Token = jwtToken;
            response.User = user;
            response.Expiration = DateTime.UtcNow.AddDays(7);

            return Ok(
                new
                {
                    succeeded = true,
                    message = "Registration successful!",
                    token = jwtToken,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                    },
                    expiration = DateTime.UtcNow.AddDays(7),
                }
            );
        }

        foreach (var error in result.Errors)
        {
            response.Errors.Add(error.Description);
        }

        return BadRequest(response);
    }

    /// <summary>
    /// Login a user
    /// </summary>
    /// <param name="model">Login credentials</param>
    /// <returns>JWT token and user info</returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginModel model)
    {
        var response = new AuthResponse();

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            response.Errors.Add("Invalid login attempt");
            return BadRequest(response);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false
        );

        if (result.Succeeded)
        {
            var token = await GenerateJwtToken(user);

            response.Succeeded = true;
            response.Token = token;
            response.User = user;
            response.Expiration = DateTime.UtcNow.AddDays(7);

            return Ok(response);
        }

        if (result.IsLockedOut)
        {
            response.Errors.Add("Account locked out");
        }
        else
        {
            response.Errors.Add("Invalid login attempt");
        }

        return BadRequest(response);
    }

    /// <summary>
    /// Logout the current user
    /// </summary>
    /// <returns>Logout confirmation</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Change user's password
    /// </summary>
    /// <param name="model">Password change data</param>
    /// <returns>Change password result</returns>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            model.CurrentPassword,
            model.NewPassword
        );

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Request a password reset for a user
    /// </summary>
    /// <param name="model">Email for password reset</param>
    /// <returns>Password reset request confirmation</returns>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return Ok(
                new
                {
                    message = "If your email is registered, you will receive a password reset link",
                }
            );
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // In development, we might want to return the token for testing
        if (
            _configuration.GetValue<bool>("ReturnTokensInDevelopment")
            && _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development"
        )
        {
            return Ok(
                new
                {
                    message = "Password reset token generated",
                    resetToken = token,
                    email = model.Email,
                }
            );
        }

        return Ok(
            new
            {
                message = "If your email is registered, a password reset token will be generated",
            }
        );
    }

    /// <summary>
    /// Reset user's password using a reset token
    /// </summary>
    /// <param name="model">Password reset data including token</param>
    /// <returns>Password reset result</returns>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return BadRequest(new { message = "Password reset failed" });
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { message = "Password has been reset successfully" });
    }

    /// <summary>
    /// Upload a new profile picture for a user
    /// </summary>
    /// <param name="model">Profile picture upload model</param>
    /// <returns>URL of the uploaded profile picture</returns>
    [HttpPost("profile-picture")]
    [Authorize]
    public async Task<ActionResult<string>> UploadProfilePicture(
        [FromForm] ProfilePictureModel model
    )
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        if (model.Picture == null)
        {
            return BadRequest(new { message = "No file was uploaded." });
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(model.Picture.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(
                new
                {
                    message = "Invalid file type. Only jpg, jpeg, png, and gif files are allowed.",
                }
            );
        }

        // Validate file size (max 5MB)
        if (model.Picture.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new { message = "File size exceeds 5MB limit." });
        }

        try
        {
            var fileName = $"{userId}_{DateTime.UtcNow.Ticks}{extension}";
            var uploadsFolder = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "uploads",
                "profile-pictures"
            );

            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Delete old profile picture if it exists
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                var oldFileName = Path.GetFileName(user.ProfilePictureUrl);
                var oldFilePath = Path.Combine(uploadsFolder, oldFileName);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.Picture.CopyToAsync(fileStream);
            }

            // Update user's profile picture URL
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var profilePictureUrl = $"{baseUrl}/uploads/profile-pictures/{fileName}";
            user.ProfilePictureUrl = profilePictureUrl;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }

            return Ok(new { profilePictureUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                new { message = "Error uploading profile picture", error = ex.Message }
            );
        }
    }

    /// <summary>
    /// Update an existing profile picture for a user
    /// </summary>
    /// <param name="model">Profile picture update model</param>
    /// <returns>URL of the updated profile picture</returns>
    [HttpPut("profile-picture")]
    [Authorize]
    public async Task<ActionResult<string>> UpdateProfilePicture(
        [FromForm] ProfilePictureModel model
    )
    {
        return await UploadProfilePicture(model);
    }

    // Helper method to generate JWT tokens
    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
        };

        // Add roles as claims
        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["JWT:Secret"] ?? "YourFallbackSecretKeyForDevelopmentOnly123456789"
            )
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"] ?? "https://localhost",
            audience: _configuration["JWT:ValidAudience"] ?? "https://localhost",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Creates a default free subscription for a newly registered user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    private async Task CreateDefaultFreeSubscription(string userId)
    {
        using var scope = HttpContext.RequestServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Context.AppDbContext>();

        // Create a free subscription
        var subscription = new Models.Subscription
        {
            UserId = userId,
            PlanName = "Free Plan",
            Price = 0,
            Currency = "USD",
            BillingFrequency = "Monthly",
            StartDate = DateTime.UtcNow,
            IsActive = true,
            AutoRenew = true,
            PaymentMethod = "None"
        };

        context.Subscription.Add(subscription);
        await context.SaveChangesAsync();
    }
}
