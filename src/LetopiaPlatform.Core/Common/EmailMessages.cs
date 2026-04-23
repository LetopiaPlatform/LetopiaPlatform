using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.Core.DTOs.Email;

namespace LetopiaPlatform.Core.Common;
public static class EmailMessages
{
    // → NEW email: confirm button + plain-text fallback
    public static EmailMessage EmailChangeConfirmation(
        string toEmail,
        string userName,
        string confirmUrl) => new(
            To: toEmail,
            Subject: "Confirm your new email address",
            Title: "Email Change Request",
            Body: "We received a request to change the email on your Letopia account. " +
                          "Click the button below to confirm. " +
                          "This link expires in <strong>24 hours</strong>.",
            UserName: userName,
            AfterCodeBody: $"Or copy this link into your browser:<br/><small>{confirmUrl}</small><br/><br/>" +
                           "If you didn't request this, please secure your account immediately.",
            ButtonText: "Confirm New Email",
            ButtonUrl: confirmUrl
        );

    // → OLD email: security notice only, no action link
    public static EmailMessage EmailChangeNotification(
        string toEmail,
        string userName,
        string newEmail) => new(
            To: toEmail,
            Subject: "Email change requested on your account",
            Title: "Security Notice",
            Body: $"A request was made to change your Letopia account email to " +
                          $"<strong>{newEmail}</strong>.<br/><br/>" +
                          "If this was you, confirm it from your new address — no action needed here. " +
                          "If this wasn't you, please secure your account immediately.",
            UserName: userName,
            AfterCodeBody: "This is a security notification. No action required."
        );
}
