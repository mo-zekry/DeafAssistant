using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeafAssistant.Models;

/// <summary>
/// Represents user feedback for the application
/// </summary>
public class Feedback
{
  /// <summary>
  /// Unique identifier for the feedback
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// ID of the user who provided the feedback
  /// </summary>
  [Required]
  public string UserId { get; set; } = string.Empty;

  /// <summary>
  /// The user who provided the feedback
  /// </summary>
  [ForeignKey("UserId")]
  public ApplicationUser User { get; set; } = new ApplicationUser();

  /// <summary>
  /// Feedback comment text
  /// </summary>
  [Required(ErrorMessage = "Please provide a comment with your feedback.")]
  [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
  public string Comment { get; set; } = string.Empty;

  /// <summary>
  /// Rating provided by the user (1-5 scale)
  /// </summary>
  [Required]
  [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
  public int Rating { get; set; }

  /// <summary>
  /// Category of the feedback (App, Content, Lessons, Technical, Other)
  /// </summary>
  [StringLength(50)]
  public string Category { get; set; } = "App";

  /// <summary>
  /// Optional ID of the lesson this feedback is related to
  /// </summary>
  public int? LessonId { get; set; }

  /// <summary>
  /// Optional lesson this feedback is related to
  /// </summary>
  [ForeignKey("LessonId")]
  public Lesson? Lesson { get; set; }

  /// <summary>
  /// Whether the feedback has been reviewed by admins
  /// </summary>
  public bool IsReviewed { get; set; } = false;

  /// <summary>
  /// Admin response to the feedback, if any
  /// </summary>
  [StringLength(1000)]
  public string? AdminResponse { get; set; }

  /// <summary>
  /// Date when admin responded to the feedback
  /// </summary>
  [DataType(DataType.DateTime)]
  public DateTime? ResponseDate { get; set; }

  /// <summary>
  /// Date and time when the feedback was created
  /// </summary>
  [DataType(DataType.DateTime)]
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
