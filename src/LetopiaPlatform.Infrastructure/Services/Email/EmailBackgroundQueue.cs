using System.Threading.Channels;
using LetopiaPlatform.Core.DTOs.Email;
using LetopiaPlatform.Core.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Infrastructure.Services.Email;

public sealed class EmailBackgroundQueue : BackgroundService, IEmailService
{
    private readonly Channel<EmailMessage> _channel = Channel.CreateBounded<EmailMessage>(100);
    private readonly Channel<EmailMessage> _deadLetterChannel = Channel.CreateBounded<EmailMessage>(100);
    private readonly SmtpEmailService _smtpService;
    private readonly ILogger<EmailBackgroundQueue> _logger;

    private const int MaxRetryAttempts = 3;
    private const int InitialBackoffMs = 1000;
    private static readonly TimeSpan DeadLetterRetryDelay = TimeSpan.FromMinutes(1);

    public EmailBackgroundQueue(
        SmtpEmailService smtpService,
        ILogger<EmailBackgroundQueue> logger)
    {
        _smtpService = smtpService;
        _logger = logger;
    }

    public void Enqueue(EmailMessage message)
    {
        if (!_channel.Writer.TryWrite(message))
        {
            _logger.LogWarning("Email queue is full. Dropping email to {To} with subject {Subject}", message.To, message.Subject);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email background queue started.");

        var emailProcessingTask = Task.Run(async () =>
        {
            await foreach (var message in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                bool sent = await TrySendWithRetryAsync(message, stoppingToken);
                if (!sent)
                {
                    _logger.LogError("Email permanently failed. Moving to dead-letter queue: {To} — Subject: {Subject}", message.To, message.Subject);
                    if (!_deadLetterChannel.Writer.TryWrite(message))
                    {
                        _logger.LogError("Dead-letter queue is full. Dropping email to {To} — Subject: {Subject}", message.To, message.Subject);
                    }
                }
            }
        }, stoppingToken);

        var deadLetterProcessingTask = Task.Run(async () =>
        {
            await foreach (var deadMessage in _deadLetterChannel.Reader.ReadAllAsync(stoppingToken))
            {
                _logger.LogWarning("Dead-letter retry: waiting {Delay} before re-sending to {To} — Subject: {Subject}",
                    DeadLetterRetryDelay, deadMessage.To, deadMessage.Subject);

                await Task.Delay(DeadLetterRetryDelay, stoppingToken);

                try
                {
                    await _smtpService.SendAsync(deadMessage, stoppingToken);
                    _logger.LogInformation("Dead-letter email sent successfully to {To} — Subject: {Subject}",
                        deadMessage.To, deadMessage.Subject);
                }
                catch (Exception ex) when (ex is not OperationCanceledException && ex is not TaskCanceledException)
                {
                    _logger.LogError(ex, "Dead-letter email permanently failed to {To} — Subject: {Subject}. Giving up.",
                        deadMessage.To, deadMessage.Subject);
                }
            }
        }, stoppingToken);

        await Task.WhenAll(emailProcessingTask, deadLetterProcessingTask);

        _logger.LogInformation("Email background queue stopped.");
    }
 
    private async Task<bool> TrySendWithRetryAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        int attempt = 0;
        int backoff = InitialBackoffMs;
        while (attempt < MaxRetryAttempts)
        {
            try
            {
                await _smtpService.SendAsync(message, cancellationToken);
                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Respect cancellation and propagate it to the caller.
                throw;
            }
            catch (Exception ex)
            {
                attempt++;
                _logger.LogWarning(ex, "Retry {Attempt}/{Max} failed for email to {To} — Subject: {Subject}", attempt, MaxRetryAttempts, message.To, message.Subject);
                if (attempt < MaxRetryAttempts)
                {
                    await Task.Delay(backoff, cancellationToken);
                    backoff *= 2;
                }
            }
        }
        return false;
    }
}