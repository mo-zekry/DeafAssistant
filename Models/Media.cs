using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeafAssistant.Models;

/// <summary>
/// Represents a media resource in the application
/// </summary>
public class Media
{
  /// <summary>
  /// Unique identifier for the media
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// Name of the media resource
  /// </summary>
  [Required]
  [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// Description of the media content
  /// </summary>
  [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
  public string Description { get; set; } = string.Empty;

  /// <summary>
  /// URL where the media is stored
  /// </summary>
  [Required]
  [Url(ErrorMessage = "Please provide a valid URL.")]
  public string Url { get; set; } = string.Empty;

  /// <summary>
  /// Type of the media (Video, Image, Audio, Document)
  /// </summary>
  [Required]
  [StringLength(20)]
  public string MediaType { get; set; } = string.Empty;

  /// <summary>
  /// Size of the media file in bytes
  /// </summary>
  public long? FileSize { get; set; }

  /// <summary>
  /// MIME type of the media file
  /// </summary>
  [StringLength(50)]
  public string? ContentType { get; set; }

  /// <summary>
  /// Optional ID of the lesson this media is associated with
  /// </summary>
  public int? LessonId { get; set; }

  /// <summary>
  /// Related lesson if this media is associated with one
  /// </summary>
  [ForeignKey("LessonId")]
  public Lesson? Lesson { get; set; }

  /// <summary>
  /// Optional user who uploaded this media
  /// </summary>
  public string? UserId { get; set; }

  /// <summary>
  /// User who uploaded this media if applicable
  /// </summary>
  [ForeignKey("UserId")]
  public ApplicationUser? User { get; set; }

  /// <summary>
  /// Date and time when the media was created
  /// </summary>
  [DataType(DataType.DateTime)]
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  /// <summary>
  /// Date and time when the media was last updated
  /// </summary>
  [DataType(DataType.DateTime)]
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
