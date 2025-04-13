using DeafAssistant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeafAssistant.Controllers;

/// <summary>
/// Controller for managing application users
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
  private readonly UserManager<ApplicationUser> _userManager;

  /// <summary>
  /// Constructor for UsersController
  /// </summary>
  /// <param name="userManager">ASP.NET Core Identity user manager</param>
  public UsersController(UserManager<ApplicationUser> userManager)
  {
    _userManager = userManager;
  }

  /// <summary>
  /// Get all users
  /// </summary>
  /// <returns>List of all users</returns>
  [HttpGet]
  public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetUsers()
  {
    return await _userManager.Users.ToListAsync();
  }

  /// <summary>
  /// Get a specific user by ID
  /// </summary>
  /// <param name="id">User ID</param>
  /// <returns>The requested user</returns>
  [HttpGet("{id}")]
  public async Task<ActionResult<ApplicationUser>> GetUser(string id)
  {
    var user = await _userManager.FindByIdAsync(id);

    if (user == null)
    {
      return NotFound();
    }

    return user;
  }

  /// <summary>
  /// Get current user profile
  /// </summary>
  /// <returns>Current user's profile</returns>
  [HttpGet("me")]
  [Authorize] // Allow any authenticated user, not just admins
  public async Task<ActionResult<ApplicationUser>> GetCurrentUser()
  {
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
    {
      return Unauthorized();
    }

    var user = await _userManager.FindByIdAsync(userId);

    if (user == null)
    {
      return NotFound();
    }

    return user;
  }

  /// <summary>
  /// Update a user
  /// </summary>
  /// <param name="id">User ID</param>
  /// <param name="userData">Updated user data</param>
  /// <returns>No content if successful</returns>
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateUser(string id, ApplicationUser userData)
  {
    if (id != userData.Id)
    {
      return BadRequest();
    }

    var user = await _userManager.FindByIdAsync(id);
    if (user == null)
    {
      return NotFound();
    }

    // Update basic properties
    // Note: This is a simplified example. In a real app, you'd need to carefully
    // control which properties the admin can update
    user.Email = userData.Email;
    user.UserName = userData.UserName;
    user.PhoneNumber = userData.PhoneNumber;

    var result = await _userManager.UpdateAsync(user);

    if (!result.Succeeded)
    {
      return BadRequest(result.Errors);
    }

    return NoContent();
  }

  /// <summary>
  /// Delete a user
  /// </summary>
  /// <param name="id">User ID to delete</param>
  /// <returns>No content if successful</returns>
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteUser(string id)
  {
    var user = await _userManager.FindByIdAsync(id);
    if (user == null)
    {
      return NotFound();
    }

    var result = await _userManager.DeleteAsync(user);

    if (!result.Succeeded)
    {
      return BadRequest(result.Errors);
    }

    return NoContent();
  }
}
