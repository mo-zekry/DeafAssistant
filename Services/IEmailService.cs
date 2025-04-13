namespace DeafAssistant.Services;

/// <summary>
/// Interface for email services
/// </summary>
public interface IEmailService
{
  /// <summary>
  /// Sends an email
  /// </summary>
  /// <param name="to">Recipient email address</param>
  /// <param name="subject">Email subject</param>
  /// <param name="body">Email body content (can be HTML)</param>
  /// <returns>A task representing the asynchronous operation</returns>
  Task SendEmailAsync(string to, string subject, string body);
}
