
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using LetopiaPlatform.Core.AppSettings;
using LetopiaPlatform.Core.DTOs.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LetopiaPlatform.Infrastructure.Services.Email;

public sealed class SmtpEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly string _templateHtml;

    public SmtpEmailService (
        IOptions<SmtpSettings> settings,
        ILogger<SmtpEmailService> logger
    )
    {
        _settings = settings.Value;
        _logger = logger;
        _templateHtml = LoadEmbeddedTemplate();
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var html = BuildHtml(message);
        using var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        mime.To.Add(MailboxAddress.Parse(message.To));
        mime.Subject = message.Subject;
        mime.Body = new TextPart("html") { Text = html };

        using var client = new SmtpClient();

        client.ServerCertificateValidationCallback = ValidateCertificate;

        await client.ConnectAsync(
            _settings.Host,
            _settings.Port,
            _settings.UseSsl ? SecureSocketOptions.StartTls
                             : SecureSocketOptions.Auto,
         cancellationToken);

         if (!string.IsNullOrEmpty(_settings.Username))
            await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
        
        await client.SendAsync(mime, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken: cancellationToken);

        _logger.LogInformation("Email sent to {To} — Subject: {Subject}", message.To, message.Subject);        
    }

    private string BuildHtml(EmailMessage message)
    {
        var buttonBlock = !string.IsNullOrEmpty(message.ButtonText) && !string.IsNullOrEmpty(message.ButtonUrl)
            ? $"""<p style="text-align:center"><a class="btn" href="{HttpUtility.HtmlAttributeEncode(message.ButtonUrl)}">{HttpUtility.HtmlEncode(message.ButtonText)}</a></p>"""
            : string.Empty;

        return _templateHtml
            .Replace("{{Subject}}", HttpUtility.HtmlEncode(message.Subject))
            .Replace("{{Title}}", HttpUtility.HtmlEncode(message.Title))
            .Replace("{{Body}}", message.Body)
            .Replace("{{ButtonBlock}}", buttonBlock)
            .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());
    }

    private static string LoadEmbeddedTemplate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .First(n => n.EndsWith("email-template.html", StringComparison.OrdinalIgnoreCase));

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("Embedded email template not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static bool ValidateCertificate(
        object sender,
        X509Certificate? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        return sslPolicyErrors == SslPolicyErrors.None;
    }
}