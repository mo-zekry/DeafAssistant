using System.ComponentModel.DataAnnotations;

namespace DeafAssistant.Models.Account;

/// <summary>
/// Model for profile picture upload/update
/// </summary>
public class ProfilePictureModel
{
    /// <summary>
    /// The profile picture file
    /// </summary>
    [Required]
    public IFormFile Picture { get; set; } = null!;
}
