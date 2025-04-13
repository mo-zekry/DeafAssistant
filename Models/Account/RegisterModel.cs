using System.ComponentModel.DataAnnotations;

namespace DeafAssistant.Models.Account;

/// <summary>
/// Model for user registration
/// </summary>
public class RegisterModel
{
  /// <summary>
  /// User's email address (used as username)
  /// </summary>
  [Required]
  [EmailAddress]
  public string Email { get; set; } = string.Empty;

  /// <summary>
  /// User's first name
  /// </summary>
  [Required]
  public string FirstName { get; set; } = string.Empty;

  /// <summary>
  /// User's last name
  /// </summary>
  [Required]
  public string LastName { get; set; } = string.Empty;

  /// <summary>
  /// User's phone number
  /// </summary>
  public string PhoneNumber { get; set; } = string.Empty;

  /// <summary>
  /// User's password
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
  /// Confirmation of the user's password
  /// </summary>
  [DataType(DataType.Password)]
  [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
  public string ConfirmPassword { get; set; } = string.Empty;

  /// <summary>
  /// User's date of birth
  /// </summary>
  [DataType(DataType.Date)]
  public DateTime Birthday { get; set; }
}
