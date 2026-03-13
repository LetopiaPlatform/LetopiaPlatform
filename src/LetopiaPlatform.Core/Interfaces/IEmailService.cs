using LetopiaPlatform.Core.DTOs.Email;

namespace LetopiaPlatform.Core.Services.Interfaces;

/// <summary>
/// Contract for an email service responsible for sending email messages. Implementations of this interface should handle the queuing and sending of emails, including any necessary retry logic and error handling.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Enqueues an email message to be sent. Implementations should handle the actual sending logic, including retries and error handling as needed.
    /// </summary>
    /// <param name="message">The email message to enqueue.</param>
    void Enqueue(EmailMessage message);
}