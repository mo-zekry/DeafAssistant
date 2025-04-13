using DeafAssistant.Context;
using DeafAssistant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeafAssistant.Controllers;

/// <summary>
/// Controller for managing lessons
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LessonsController : ControllerBase
{
  private readonly AppDbContext _context;

  /// <summary>
  /// Constructor for LessonsController
  /// </summary>
  /// <param name="context">Application database context</param>
  public LessonsController(AppDbContext context)
  {
    _context = context;
  }

  /// <summary>
  /// Get all lessons
  /// </summary>
  /// <returns>List of all lessons</returns>
  [HttpGet]
  public async Task<ActionResult<IEnumerable<Lesson>>> GetLessons()
  {
    return await _context.Lesson.ToListAsync();
  }

  /// <summary>
  /// Get a specific lesson by ID
  /// </summary>
  /// <param name="id">Lesson ID</param>
  /// <returns>The requested lesson</returns>
  [HttpGet("{id}")]
  public async Task<ActionResult<Lesson>> GetLesson(int id)
  {
    var lesson = await _context.Lesson.FindAsync(id);

    if (lesson == null)
    {
      return NotFound();
    }

    return lesson;
  }

  /// <summary>
  /// Create a new lesson
  /// </summary>
  /// <param name="lesson">Lesson data</param>
  /// <returns>Created lesson with ID</returns>
  [HttpPost]
  [Authorize(Roles = "Admin")]
  public async Task<ActionResult<Lesson>> CreateLesson(Lesson lesson)
  {
    lesson.CreatedAt = DateTime.UtcNow;
    lesson.UpdatedAt = DateTime.UtcNow;

    _context.Lesson.Add(lesson);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetLesson), new { id = lesson.Id }, lesson);
  }

  /// <summary>
  /// Update an existing lesson
  /// </summary>
  /// <param name="id">Lesson ID</param>
  /// <param name="lesson">Updated lesson data</param>
  /// <returns>No content if successful</returns>
  [HttpPut("{id}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> UpdateLesson(int id, Lesson lesson)
  {
    if (id != lesson.Id)
    {
      return BadRequest();
    }

    lesson.UpdatedAt = DateTime.UtcNow;
    _context.Entry(lesson).State = EntityState.Modified;
    _context.Entry(lesson).Property(x => x.CreatedAt).IsModified = false;

    try
    {
      await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
      if (!LessonExists(id))
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
  /// Delete a lesson
  /// </summary>
  /// <param name="id">Lesson ID to delete</param>
  /// <returns>No content if successful</returns>
  [HttpDelete("{id}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> DeleteLesson(int id)
  {
    var lesson = await _context.Lesson.FindAsync(id);
    if (lesson == null)
    {
      return NotFound();
    }

    _context.Lesson.Remove(lesson);
    await _context.SaveChangesAsync();

    return NoContent();
  }

  private bool LessonExists(int id)
  {
    return _context.Lesson.Any(e => e.Id == id);
  }
}
