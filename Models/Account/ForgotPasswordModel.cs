using System.ComponentModel.DataAnnotations;

namespace DeafAssistant.Models.Account;

/// <summary>
/// Model for requesting a password reset
/// </summary>
public class ForgotPasswordModel
{
  /// <summary>
  /// User's email address
  /// </summary>
  [Required]
  [EmailAddress]
  public string Email { get; set; } = string.Empty;
}
