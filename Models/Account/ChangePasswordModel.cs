using System.ComponentModel.DataAnnotations;

namespace DeafAssistant.Models.Account;

/// <summary>
/// Model for changing password
/// </summary>
public class ChangePasswordModel
{
  /// <summary>
  /// User's current password
  /// </summary>
  [Required]
  [DataType(DataType.Password)]
  public string CurrentPassword { get; set; } = string.Empty;

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
  public string NewPassword { get; set; } = string.Empty;

  /// <summary>
  /// Confirmation of the user's new password
  /// </summary>
  [DataType(DataType.Password)]
  [Compare(
    "NewPassword",
    ErrorMessage = "The new password and confirmation password do not match."
  )]
  public string ConfirmPassword { get; set; } = string.Empty;
}
