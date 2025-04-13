using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeafAssistant.Models;

/// <summary>
/// Represents a refresh token for JWT authentication
/// </summary>
public class UserRefreshToken
{
  /// <summary>
  /// Unique identifier for the refresh token
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// ID of the user this token belongs to
  /// </summary>
  [Required]
  public string UserId { get; set; } = string.Empty;

  /// <summary>
  /// User this token belongs to
  /// </summary>
  [ForeignKey("UserId")]
  public ApplicationUser? User { get; set; }

  /// <summary>
  /// The refresh token value
  /// </summary>
  [Required]
  public string RefreshToken { get; set; } = string.Empty;

  /// <summary>
  /// JWT token associated with this refresh token
  /// </summary>
  [Required]
  public string JwtId { get; set; } = string.Empty;

  /// <summary>
  /// When the refresh token expires
  /// </summary>
  [Required]
  [DataType(DataType.DateTime)]
  public DateTime ExpiryDate { get; set; }

  /// <summary>
  /// When the refresh token was created
  /// </summary>
  [DataType(DataType.DateTime)]
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  /// <summary>
  /// IP address used when the token was issued
  /// </summary>
  [StringLength(50)]
  public string? IssuedByIp { get; set; }

  /// <summary>
  /// Whether this token has been used
  /// </summary>
  public bool IsUsed { get; set; } = false;

  /// <summary>
  /// Whether this token has been revoked
  /// </summary>
  public bool IsRevoked { get; set; } = false;

  /// <summary>
  /// Date when the token was used or revoked
  /// </summary>
  [DataType(DataType.DateTime)]
  public DateTime? RevokedAt { get; set; }

  /// <summary>
  /// Checks if the token is active (not expired, not used, and not revoked)
  /// </summary>
  [NotMapped]
  public bool IsActive => !IsRevoked && !IsUsed && ExpiryDate > DateTime.UtcNow;
}
