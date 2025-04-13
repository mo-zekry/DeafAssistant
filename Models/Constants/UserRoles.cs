namespace DeafAssistant.Models.Constants;

/// <summary>
/// Defines constants for user roles within the application
/// </summary>
public static class UserRoles
{
  /// <summary>
  /// Administrator role with full system access
  /// </summary>
  public const string Admin = "Admin";

  /// <summary>
  /// Standard user role for registered users
  /// </summary>
  public const string User = "User";

  /// <summary>
  /// Instructor role for users who can create and manage lessons
  /// </summary>
  public const string Instructor = "Instructor";

  /// <summary>
  /// Premium user role for subscribers with additional features
  /// </summary>
  public const string Premium = "Premium";

  /// <summary>
  /// Moderator role for users who can moderate content and feedback
  /// </summary>
  public const string Moderator = "Moderator";

  /// <summary>
  /// Returns array of all available roles in the system
  /// </summary>
  public static string[] GetAllRoles()
  {
    return new[] { Admin, User, Instructor, Premium, Moderator };
  }
}
