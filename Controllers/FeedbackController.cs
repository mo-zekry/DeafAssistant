using DeafAssistant.Context;
using DeafAssistant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeafAssistant.Controllers;

/// <summary>
/// Controller for managing user feedback
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
  private readonly AppDbContext _context;

  /// <summary>
  /// Constructor for FeedbackController
  /// </summary>
  /// <param name="context">Application database context</param>
  public FeedbackController(AppDbContext context)
  {
    _context = context;
  }

  /// <summary>
  /// Get all feedback
  /// </summary>
  /// <returns>List of all feedback</returns>
  [HttpGet]
  [Authorize(Roles = "Admin")]
  public async Task<ActionResult<IEnumerable<Feedback>>> GetFeedback()
  {
    return await _context.Feedback.Include(f => f.User).ToListAsync();
  }

  /// <summary>
  /// Get a specific feedback by ID
  /// </summary>
  /// <param name="id">Feedback ID</param>
  /// <returns>The requested feedback</returns>
  [HttpGet("{id}")]
  [Authorize(Roles = "Admin")]
  public async Task<ActionResult<Feedback>> GetFeedback(int id)
  {
    var feedback = await _context
      .Feedback.Include(f => f.User)
      .FirstOrDefaultAsync(f => f.Id == id);

    if (feedback == null)
    {
      return NotFound();
    }

    return feedback;
  }

  /// <summary>
  /// Submit new feedback
  /// </summary>
  /// <param name="feedback">Feedback data</param>
  /// <returns>Created feedback with ID</returns>
  [HttpPost]
  [Authorize]
  public async Task<ActionResult<Feedback>> CreateFeedback(Feedback feedback)
  {
    // Get current user ID from claims
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
    {
      return Unauthorized();
    }

    feedback.UserId = userId;
    feedback.CreatedAt = DateTime.UtcNow;

    _context.Feedback.Add(feedback);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetFeedback), new { id = feedback.Id }, feedback);
  }

  /// <summary>
  /// Delete feedback
  /// </summary>
  /// <param name="id">Feedback ID to delete</param>
  /// <returns>No content if successful</returns>
  [HttpDelete("{id}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> DeleteFeedback(int id)
  {
    var feedback = await _context.Feedback.FindAsync(id);
    if (feedback == null)
    {
      return NotFound();
    }

    _context.Feedback.Remove(feedback);
    await _context.SaveChangesAsync();

    return NoContent();
  }
}
