using DeafAssistant.Models;
using DeafAssistant.Models.Constants;
using Microsoft.AspNetCore.Identity;

namespace DeafAssistant.Services;

/// <summary>
/// Service responsible for initializing application roles and default admin user
/// </summary>
public class RoleInitializerService
{
  private readonly RoleManager<IdentityRole> _roleManager;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly IConfiguration _configuration;
  private readonly ILogger<RoleInitializerService> _logger;

  /// <summary>
  /// Constructor for RoleInitializerService
  /// </summary>
  public RoleInitializerService(
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    ILogger<RoleInitializerService> logger
  )
  {
    _roleManager = roleManager;
    _userManager = userManager;
    _configuration = configuration;
    _logger = logger;
  }

  /// <summary>
  /// Initialize roles and admin user in the database
  /// </summary>
  public async Task InitializeAsync()
  {
    try
    {
      _logger.LogInformation("Starting role initialization");

      // Create roles if they don't exist
      await CreateRolesAsync();

      // Create admin user if it doesn't exist
      await CreateAdminUserAsync();

      _logger.LogInformation("Role initialization completed successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred while initializing roles");
    }
  }

  private async Task CreateRolesAsync()
  {
    // Create all roles defined in UserRoles class
    foreach (var roleName in UserRoles.GetAllRoles())
    {
      if (!await _roleManager.RoleExistsAsync(roleName))
      {
        _logger.LogInformation("Creating role: {RoleName}", roleName);
        await _roleManager.CreateAsync(new IdentityRole(roleName));
      }
    }
  }

  private async Task CreateAdminUserAsync()
  {
    // Get admin credentials from configuration
    var adminEmail = _configuration["AdminUser:Email"] ?? "admin@deafassistant.com";
    var adminPassword = _configuration["AdminUser:Password"] ?? "Admin@123456";
    var adminFirstName = _configuration["AdminUser:FirstName"] ?? "System";
    var adminLastName = _configuration["AdminUser:LastName"] ?? "Administrator";

    // Check if admin user exists
    var adminUser = await _userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
      _logger.LogInformation("Creating default admin user");

      // Create admin user
      adminUser = new ApplicationUser
      {
        UserName = adminEmail,
        Email = adminEmail,
        EmailConfirmed = true,
        FirstName = adminFirstName,
        LastName = adminLastName,
        Role = UserRoles.Admin,
        CreatedAt = DateTime.UtcNow,
      };

      var result = await _userManager.CreateAsync(adminUser, adminPassword);

      if (result.Succeeded)
      {
        _logger.LogInformation("Admin user created successfully");

        // Add admin role to the user
        await _userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
      }
      else
      {
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogError("Failed to create admin user: {Errors}", errors);
      }
    }
  }
}
