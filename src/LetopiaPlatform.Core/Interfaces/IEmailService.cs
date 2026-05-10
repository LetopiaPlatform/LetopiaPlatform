using LetopiaPlatform.Core.DTOs.Email;

namespace LetopiaPlatform.Core.Services.Interfaces;

/// <summary>
/// Contract for an email service responsible for sending email messages.
/// Implementations should handle queuing, retry logic, and error handling.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Enqueues an email message to be sent.
    /// Implementations should handle the actual sending logic, including retries and error handling.
    /// </summary>
    /// <param name="message">The email message to enqueue.</param>
    void Enqueue(EmailMessage message);

    /// <summary>
    /// Sends a confirmation link to the user's new email address.
    /// The link must be clicked to apply the email change.
    /// </summary>
    /// <param name="toEmail">The new email address to send the confirmation to.</param>
    /// <param name="userName">The display name of the user, used in the email greeting.</param>
    /// <param name="confirmUrl">The full confirmation URL containing the token and userId.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendEmailChangeConfirmationAsync(
        string toEmail,
        string userName,
        string confirmUrl,
        CancellationToken ct = default);

    /// <summary>
    /// Sends a security notification to the user's current (old) email address
    /// informing them that an email change was requested.
    /// Contains no action link — for awareness only.
    /// </summary>
    /// <param name="toEmail">The current email address to notify.</param>
    /// <param name="userName">The display name of the user, used in the email greeting.</param>
    /// <param name="newEmail">The new email address the change was requested for.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendEmailChangeNotificationAsync(
        string toEmail,
        string userName,
        string newEmail,
        CancellationToken ct = default);
}
