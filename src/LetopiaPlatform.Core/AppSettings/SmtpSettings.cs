namespace LetopiaPlatform.Core.AppSettings;

public sealed class SmtpSettings
{
    public const string SectionName = "SmtpSettings";
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = "Letopia";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = true;
    public string EmailAssetsBaseUrl { get; set; } = string.Empty;
    public string FrontendBaseUrl { get; set; } = string.Empty;
}