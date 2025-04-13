using DeafAssistant.Context;
using DeafAssistant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeafAssistant.Controllers;

/// <summary>
/// Controller for managing user subscriptions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
  private readonly AppDbContext _context;

  /// <summary>
  /// Constructor for SubscriptionsController
  /// </summary>
  /// <param name="context">Application database context</param>
  public SubscriptionsController(AppDbContext context)
  {
    _context = context;
  }

  /// <summary>
  /// Get all subscriptions (admin only)
  /// </summary>
  /// <returns>List of all subscriptions</returns>
  [HttpGet]
  [Authorize(Roles = "Admin")]
  public async Task<ActionResult<IEnumerable<Subscription>>> GetSubscriptions()
  {
    return await _context.Subscription.Include(s => s.User).ToListAsync();
  }

  /// <summary>
  /// Get a specific subscription by ID
  /// </summary>
  /// <param name="id">Subscription ID</param>
  /// <returns>The requested subscription</returns>
  [HttpGet("{id}")]
  public async Task<ActionResult<Subscription>> GetSubscription(int id)
  {
    // Get current user ID from claims
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    var subscription = await _context
      .Subscription.Include(s => s.User)
      .FirstOrDefaultAsync(s => s.Id == id);

    if (subscription == null)
    {
      return NotFound();
    }

    // Non-admin users can only access their own subscriptions
    if (!User.IsInRole("Admin") && subscription.UserId != userId)
    {
      return Forbid();
    }

    return subscription;
  }

  /// <summary>
  /// Get current user's subscription
  /// </summary>
  /// <returns>The current user's subscription</returns>
  [HttpGet("me")]
  public async Task<ActionResult<Subscription>> GetMySubscription()
  {
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
    {
      return Unauthorized();
    }

    var subscription = await _context
      .Subscription.Include(s => s.User)
      .FirstOrDefaultAsync(s => s.UserId == userId);

    if (subscription == null)
    {
      return NotFound();
    }

    return subscription;
  }

  /// <summary>
  /// Create a new subscription (admin only)
  /// </summary>
  /// <param name="subscription">Subscription data</param>
  /// <returns>Created subscription with ID</returns>
  [HttpPost]
  [Authorize(Roles = "Admin")]
  public async Task<ActionResult<Subscription>> CreateSubscription(Subscription subscription)
  {
    subscription.StartDate = DateTime.UtcNow;

    _context.Subscription.Add(subscription);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetSubscription), new { id = subscription.Id }, subscription);
  }

  /// <summary>
  /// Update an existing subscription (admin only)
  /// </summary>
  /// <param name="id">Subscription ID</param>
  /// <param name="subscription">Updated subscription data</param>
  /// <returns>No content if successful</returns>
  [HttpPut("{id}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> UpdateSubscription(int id, Subscription subscription)
  {
    if (id != subscription.Id)
    {
      return BadRequest();
    }

    _context.Entry(subscription).State = EntityState.Modified;

    try
    {
      await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
      if (!SubscriptionExists(id))
      {
        return NotFound();
      }
      else
      {
        throw;
      }
    }

    return NoContent();
  }

  /// <summary>
  /// Delete a subscription (admin only)
  /// </summary>
  /// <param name="id">Subscription ID to delete</param>
  /// <returns>No content if successful</returns>
  [HttpDelete("{id}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> DeleteSubscription(int id)
  {
    var subscription = await _context.Subscription.FindAsync(id);
    if (subscription == null)
    {
      return NotFound();
    }

    _context.Subscription.Remove(subscription);
    await _context.SaveChangesAsync();

    return NoContent();
  }

  private bool SubscriptionExists(int id)
  {
    return _context.Subscription.Any(e => e.Id == id);
  }
}
