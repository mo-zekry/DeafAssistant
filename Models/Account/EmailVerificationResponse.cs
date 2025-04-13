namespace DeafAssistant.Models.Account;

/// <summary>
/// Response model for email verification operations
/// </summary>
public class EmailVerificationResponse
{
  /// <summary>
  /// Indicates if the operation was successful
  /// </summary>
  public bool Succeeded { get; set; }

  /// <summary>
  /// Message describing the result of the operation
  /// </summary>
  public string Message { get; set; } = string.Empty;

  /// <summary>
  /// Error messages if operation failed
  /// </summary>
  public List<string> Errors { get; set; } = new List<string>();
}
