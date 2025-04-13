using System.ComponentModel.DataAnnotations;

namespace DeafAssistant.Models.Account;

/// <summary>
/// Model for resetting a password with a token
/// </summary>
public class ResetPasswordModel
{
  /// <summary>
  /// User's email address
  /// </summary>
  [Required]
  [EmailAddress]
  public string Email { get; set; } = string.Empty;

  /// <summary>
  /// Password reset token
  /// </summary>
  [Required]
  public string Token { get; set; } = string.Empty;

  /// <summary>
  /// User's new password
  /// </summary>
  [Required]
  [StringLength(
    100,
    ErrorMessage = "The {0} must be at least {2} characters long.",
    MinimumLength = 6
  )]
  [DataType(DataType.Password)]
  public string Password { get; set; } = string.Empty;

  /// <summary>
  /// Confirmation of the user's new password
  /// </summary>
  [DataType(DataType.Password)]
  [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
  public string ConfirmPassword { get; set; } = string.Empty;
}
