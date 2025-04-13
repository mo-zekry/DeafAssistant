using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DeafAssistant.Services;

/// <summary>
/// Implementation of IEmailService using SMTP for sending emails
/// </summary>
public class SmtpEmailService : IEmailService
{
  private readonly IConfiguration _configuration;
  private readonly ILogger<SmtpEmailService> _logger;

  /// <summary>
  /// Constructor for SmtpEmailService
  /// </summary>
  /// <param name="configuration">Application configuration</param>
  /// <param name="logger">Logger instance</param>
  public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
  {
    _configuration = configuration;
    _logger = logger;
  }

  /// <summary>
  /// Sends an email using SMTP
  /// </summary>
  /// <param name="to">Recipient email address</param>
  /// <param name="subject">Email subject</param>
  /// <param name="body">Email body (HTML)</param>
  public async Task SendEmailAsync(string to, string subject, string body)
  {
    try
    {
      using var message = new MailMessage();

      // Set sender from configuration
      var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "deaf.assistant@outlook.sa";
      var senderName = _configuration["EmailSettings:SenderName"] ?? "Deaf Assistant";
      message.From = new MailAddress(senderEmail, senderName);

      message.To.Add(new MailAddress(to));
      message.Subject = subject;
      message.Body = body;
      message.IsBodyHtml = true;

      using var client = new SmtpClient();

      // Configure SMTP client
      client.Host = _configuration["EmailSettings:SmtpServer"] ?? "smtp.office365.com";
      client.Port = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
      client.EnableSsl = true;
      client.DeliveryMethod = SmtpDeliveryMethod.Network;

      var username = _configuration["EmailSettings:SmtpUsername"];
      var password = _configuration["EmailSettings:SmtpPassword"];

      if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
      {
        _logger.LogInformation("Attempting authentication with username: {Username}", username);

        try
        {
          // Note: For Microsoft 365/Outlook accounts, you might need OAuth2 authentication
          // instead of simple password authentication in the future.
          // See: https://learn.microsoft.com/en-us/exchange/client-developer/legacy-protocols/how-to-authenticate-an-imap-pop-smtp-application-by-using-oauth
          client.Credentials = new NetworkCredential(username, password);
          _logger.LogInformation("Authentication successful");
        }
        catch (Exception authEx)
        {
          _logger.LogError(
            authEx,
            "Authentication failed with error code: {Message}",
            authEx.Message
          );
          if (authEx.Message.Contains("5.7.3"))
          {
            _logger.LogError(
              "This appears to be an Office 365 authentication policy issue. "
                + "You may need to create a new app password or enable less secure apps "
                + "in your Microsoft account settings."
            );
          }
          throw;
        }
      }
      else
      {
        _logger.LogWarning("No SMTP credentials provided");
      }

      // Send email
      await client.SendMailAsync(message);
      _logger.LogInformation("Email sent successfully to {EmailAddress}", to);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to send email to {EmailAddress}", to);
      throw;
    }
  }
}
