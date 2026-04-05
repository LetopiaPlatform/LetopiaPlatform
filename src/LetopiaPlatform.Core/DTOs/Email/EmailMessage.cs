namespace LetopiaPlatform.Core.DTOs.Email;

public sealed record EmailMessage(
    string To,
    string Subject,
    string Title,
    string Body,
    string? ButtonText = null,
    string? ButtonUrl = null
);