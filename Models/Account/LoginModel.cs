using System.ComponentModel.DataAnnotations;

namespace DeafAssistant.Models.Account;

/// <summary>
/// Model for user login
/// </summary>
public class LoginModel
{
  /// <summary>
  /// User's email address
  /// </summary>
  [Required]
  [EmailAddress]
  public string Email { get; set; } = string.Empty;

  /// <summary>
  /// User's password
  /// </summary>
  [Required]
  [DataType(DataType.Password)]
  public string Password { get; set; } = string.Empty;

  /// <summary>
  /// Flag to remember the user
  /// </summary>
  public bool RememberMe { get; set; }
}
