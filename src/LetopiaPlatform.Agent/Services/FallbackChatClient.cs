using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Agent.Services;

/// <summary>
/// A delegating <see cref="IChatClient"/> that tries the primary provider first
/// and falls back to a secondary provider on known provider failures
/// (transport errors, timeouts, rate-limits, and SDK bugs).
/// App-level bugs propagate up for proper debugging — they are NOT silently swallowed.
/// </summary>
public sealed class FallbackChatClient : DelegatingChatClient
{
    private readonly IChatClient _fallback;
    private readonly ILogger<FallbackChatClient> _logger;
    private readonly int _primaryTimeoutSeconds;
    private long _circuitBreakerUntilTicks = DateTimeOffset.MinValue.UtcTicks;

    private DateTimeOffset CircuitBreakerUntil => new DateTimeOffset(Interlocked.Read(ref _circuitBreakerUntilTicks), TimeSpan.Zero);

    public FallbackChatClient(
        IChatClient primary,
        IChatClient fallback,
        int primaryTimeoutSeconds,
        ILogger<FallbackChatClient> logger)
        : base(primary)
    {
        _fallback = fallback;
        _primaryTimeoutSeconds = primaryTimeoutSeconds;
        _logger = logger;
    }

    public override async Task<ChatCompletion> CompleteAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var circuitBreakerUntil = CircuitBreakerUntil;
        if (DateTimeOffset.UtcNow < circuitBreakerUntil)
        {
            _logger.LogWarning("Primary provider is on circuit breaker until {Time}. Routing directly to secondary provider.", circuitBreakerUntil);
            return await _fallback.CompleteAsync(chatMessages, options, cancellationToken);
        }

        try
        {
            _logger.LogDebug("Attempting CompleteAsync with primary provider.");
            var result = await base.CompleteAsync(chatMessages, options, cancellationToken);
            _logger.LogDebug("Primary provider CompleteAsync succeeded.");
            return result;
        }
        catch (Exception ex) when (ex is OperationCanceledException && cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (IsProviderFailure(ex))
        {
            Interlocked.Exchange(ref _circuitBreakerUntilTicks, DateTimeOffset.UtcNow.AddMinutes(5).UtcTicks);
            _logger.LogWarning(ex,
                "Primary provider CompleteAsync failed ({ExceptionType}). "
                + "Circuit breaker opened for 5 minutes. Falling back to secondary.",
                ex.GetType().Name);
            return await _fallback.CompleteAsync(chatMessages, options, cancellationToken);
        }
    }

    public override async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (DateTimeOffset.UtcNow < CircuitBreakerUntil)
        {
            _logger.LogWarning("Primary provider is on circuit breaker. Routing directly to secondary provider.");
            await foreach (var update in _fallback.CompleteStreamingAsync(chatMessages, options, cancellationToken))
            {
                yield return update;
            }
            yield break;
        }

        // Buffer primary deltas internally to avoid partial-content duplication
        // when the primary provider fails mid-stream.
        var primaryBuffer = new List<StreamingChatCompletionUpdate>();
        bool useFallback = false;
        bool retriedPrimary = false;

        // Apply a strict timeout for the primary provider
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_primaryTimeoutSeconds));

        _logger.LogDebug("Attempting CompleteStreamingAsync with primary provider ({Timeout}s timeout).", _primaryTimeoutSeconds);

        try
        {
            while (true)
            {
                var enumerator = base.CompleteStreamingAsync(chatMessages, options, timeoutCts.Token)
                    .GetAsyncEnumerator(timeoutCts.Token);

                try
                {
                    while (true)
                    {
                        bool moved;
                        try
                        {
                            moved = await enumerator.MoveNextAsync();
                        }
                        catch (Exception ex) when (ex is OperationCanceledException && cancellationToken.IsCancellationRequested)
                        {
                            throw;
                        }
                        catch (Exception ex) when (IsProviderFailure(ex) && !retriedPrimary)
                        {
                            _logger.LogWarning(ex,
                                "Primary provider failed mid-stream after {BufferedChunks} chunk(s) "
                                + "({ExceptionType}). Retrying once before opening circuit breaker.",
                                primaryBuffer.Count, ex.GetType().Name);
                            retriedPrimary = true;
                            primaryBuffer.Clear();
                            break; // break inner loop to retry with new enumerator
                        }
                        catch (Exception ex) when (IsProviderFailure(ex) && retriedPrimary)
                        {
                            Interlocked.Exchange(ref _circuitBreakerUntilTicks, DateTimeOffset.UtcNow.AddMinutes(5).UtcTicks);
                            _logger.LogWarning(ex,
                                "Primary provider failed again after retry, {BufferedChunks} chunk(s) "
                                + "({ExceptionType}). Circuit breaker opened for 5 minutes. "
                                + "Discarding buffered deltas and falling back to secondary provider.",
                                primaryBuffer.Count, ex.GetType().Name);
                            useFallback = true;
                            break; // break inner loop
                        }

                        if (!moved)
                        {
                            // Primary completed successfully
                            goto primaryDone;
                        }
                        primaryBuffer.Add(enumerator.Current);
                    }
                }
                finally
                {
                    await enumerator.DisposeAsync();
                }

                if (useFallback)
                    break;

                // If we reach here, retriedPrimary was set — continue outer loop to retry
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException && cancellationToken.IsCancellationRequested)
        {
            throw;
        }

        primaryDone:

        if (useFallback)
        {
            // Discard the buffer — replay the entire response from the fallback
            _logger.LogInformation("Starting fallback streaming from secondary provider.");
            await foreach (var update in _fallback.CompleteStreamingAsync(chatMessages, options, cancellationToken))
            {
                yield return update;
            }
        }
        else
        {
            // Primary completed successfully — yield the buffered deltas
            _logger.LogDebug(
                "Primary provider CompleteStreamingAsync succeeded with {ChunkCount} chunk(s).",
                primaryBuffer.Count);
            foreach (var update in primaryBuffer)
            {
                yield return update;
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _fallback.Dispose();

        base.Dispose(disposing);
    }

    /// <summary>
    /// Classifies whether an exception is a provider-level failure that should trigger fallback.
    /// App-level bugs (JsonException, ArgumentNullException, InvalidOperationException, etc.)
    /// are NOT classified as provider failures — they propagate up for proper debugging.
    /// </summary>
    private static bool IsProviderFailure(Exception ex)
    {
        return ex switch
        {
            // Timeouts: internal CTS timeout or HttpClient timeout
            OperationCanceledException => true,

            // HTTP transport errors: 429 rate-limit, 5xx server errors, connection drops
            HttpRequestException => true,

            // TCP connection failures
            SocketException => true,

            // Broken pipe / stream read failures mid-transfer
            IOException => true,

            // Known OpenAI SDK bug: NullRef when Gemini returns empty streaming chunks.
            // We match on the stack trace to avoid catching our own NullRef bugs.
            NullReferenceException
                when ex.StackTrace?.Contains("StreamingChatCompletionUpdate") == true
                => true,

            // OpenAI SDK's ClientResultException for API-level errors (rate limits, server errors)
            _ when ex.GetType().Name == "ClientResultException" => true,

            _ => false
        };
    }
}
