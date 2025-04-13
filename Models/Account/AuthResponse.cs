namespace DeafAssistant.Models.Account;

/// <summary>
/// Response model for authentication operations
/// </summary>
public class AuthResponse
{
  /// <summary>
  /// Indicates if the operation was successful
  /// </summary>
  public bool Succeeded { get; set; }

  /// <summary>
  /// JWT token for authenticated requests
  /// </summary>
  public string? Token { get; set; }

  /// <summary>
  /// Expiration time of the token
  /// </summary>
  public DateTime? Expiration { get; set; }

  /// <summary>
  /// User information
  /// </summary>
  public ApplicationUser? User { get; set; }

  /// <summary>
  /// Error messages if operation failed
  /// </summary>
  public List<string> Errors { get; set; } = new List<string>();
}
