using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DeafAssistant.Models;

/// <summary>
/// Represents a learning lesson in the application
/// </summary>
public class Lesson
{
  /// <summary>
  /// Unique identifier for the lesson
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// Title of the lesson
  /// </summary>
  [Required]
  [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
  public string Title { get; set; } = string.Empty;

  /// <summary>
  /// Brief description of the lesson
  /// </summary>
  [Required]
  [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
  public string Description { get; set; } = string.Empty;

  /// <summary>
  /// Main content of the lesson
  /// </summary>
  [Required]
  public string Content { get; set; } = string.Empty;

  /// <summary>
  /// Category of the lesson (e.g., Alphabet, Numbers, Conversation)
  /// </summary>
  [Required]
  [StringLength(50)]
  public string Category { get; set; } = string.Empty;

  /// <summary>
  /// Difficulty level of the lesson
  /// </summary>
  [Required]
  [Range(1, 5, ErrorMessage = "Difficulty level must be between 1 (Beginner) and 5 (Expert).")]
  public int DifficultyLevel { get; set; } = 1;

  /// <summary>
  /// Date and time when the lesson was created
  /// </summary>
  [DataType(DataType.DateTime)]
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  /// <summary>
  /// Date and time when the lesson was last updated
  /// </summary>
  [DataType(DataType.DateTime)]
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  /// <summary>
  /// Estimated duration of the lesson in minutes
  /// </summary>
  [Required]
  [Range(1, 300, ErrorMessage = "Duration must be between 1 and 300 minutes.")]
  public int DurationInMinutes { get; set; }

  /// <summary>
  /// Optional image URL for the lesson
  /// </summary>
  [Url(ErrorMessage = "Please provide a valid URL for the image.")]
  public string? ImageUrl { get; set; }

  /// <summary>
  /// Optional video URL for the lesson
  /// </summary>
  [Url(ErrorMessage = "Please provide a valid URL for the video.")]
  public string? VideoUrl { get; set; }

  /// <summary>
  /// Related media resources for this lesson
  /// </summary>
  [JsonIgnore]
  public ICollection<Media> MediaResources { get; set; } = new List<Media>();
}
