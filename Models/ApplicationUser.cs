using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DeafAssistant.Models;

/// <summary>
/// Custom user model for the Deaf Assistant application
/// </summary>
public class ApplicationUser : IdentityUser
{
  /// <summary>
  /// User's first name
  /// </summary>
  [Required]
  [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
  public string FirstName { get; set; } = string.Empty;

  /// <summary>
  /// User's last name
  /// </summary>
  [Required]
  [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
  public string LastName { get; set; } = string.Empty;

  /// <summary>
  /// User's role in the application (used alongside Identity roles)
  /// </summary>
  public string Role { get; set; } = string.Empty;

  /// <summary>
  /// User profile picture URL
  /// </summary>
  [Url(ErrorMessage = "Please provide a valid URL for the profile picture.")]
  public string ProfilePictureUrl { get; set; } = string.Empty;

  /// <summary>
  /// User's date of birth
  /// </summary>
  [DataType(DataType.Date)]
  public DateTime Birthday { get; set; }

  /// <summary>
  /// Date when the user profile was created
  /// </summary>
  [DataType(DataType.DateTime)]
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  /// <summary>
  /// Indicates whether an email send attempt was made during registration
  /// </summary>
  public bool EmailSendAttempted { get; set; } = false;

  /// <summary>
  /// Full name of the user (calculated property)
  /// </summary>
  [Display(Name = "Full Name")]
  public string FullName => $"{FirstName} {LastName}".Trim();

  /// <summary>
  /// Age of the user calculated from birthday
  /// </summary>
  public int Age => CalculateAge(Birthday);

  private int CalculateAge(DateTime birthDate)
  {
    var today = DateTime.Today;
    var age = today.Year - birthDate.Year;
    if (birthDate.Date > today.AddYears(-age))
      age--;
    return age;
  }
}
