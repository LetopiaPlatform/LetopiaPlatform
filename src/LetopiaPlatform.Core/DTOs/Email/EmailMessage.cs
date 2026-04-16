namespace LetopiaPlatform.Core.DTOs.Email;

public sealed record EmailMessage(
    string To,
    string Subject,
    string Title,
    string Body,
    string? UserName = null,
    string? Code = null,
    string? AfterCodeBody = null,
    string? ButtonText = null,
    string? ButtonUrl = null,
    string? IllustrationUrl = null
);