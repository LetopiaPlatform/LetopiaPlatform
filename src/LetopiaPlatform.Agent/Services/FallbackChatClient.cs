using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Agent.Services;

/// <summary>
/// A delegating <see cref="IChatClient"/> that tries the primary provider first
/// and falls back to a secondary on retriable errors (429, 5xx, network failures,
/// timeouts, and broken pipes).
/// Does not fall back on 400 (Bad Request) errors.
/// </summary>
public sealed class FallbackChatClient : DelegatingChatClient
{
    private readonly IChatClient _fallback;
    private readonly ILogger<FallbackChatClient> _logger;
    private long _circuitBreakerUntilTicks = DateTimeOffset.MinValue.UtcTicks;

    private DateTimeOffset CircuitBreakerUntil => new DateTimeOffset(Interlocked.Read(ref _circuitBreakerUntilTicks), TimeSpan.Zero);

    public FallbackChatClient(
        IChatClient primary,
        IChatClient fallback,
        ILogger<FallbackChatClient> logger)
        : base(primary)
    {
        _fallback = fallback;
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
        catch (Exception ex) when (IsRetriable(ex))
        {
            Interlocked.Exchange(ref _circuitBreakerUntilTicks, DateTimeOffset.UtcNow.AddMinutes(5).UtcTicks);
            _logger.LogWarning(ex,
                "Primary provider CompleteAsync failed. Circuit breaker opened for 5 minutes. Falling back to secondary.");
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

        // Apply a strict 30-second timeout for the primary provider
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));

        var enumerator = base.CompleteStreamingAsync(chatMessages, options, timeoutCts.Token)
            .GetAsyncEnumerator(timeoutCts.Token);

        _logger.LogDebug("Attempting CompleteStreamingAsync with primary provider (30s timeout).");

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
                catch (Exception ex) when (IsRetriable(ex) || ex is OperationCanceledException)
                {
                    Interlocked.Exchange(ref _circuitBreakerUntilTicks, DateTimeOffset.UtcNow.AddMinutes(5).UtcTicks);
                    _logger.LogWarning(ex,
                        "Primary provider failed mid-stream after {BufferedChunks} chunk(s) or timed out. "
                        + "Circuit breaker opened for 5 minutes. Discarding buffered deltas and falling back to secondary provider.",
                        primaryBuffer.Count);
                    useFallback = true;
                    break;
                }

                if (!moved) break;
                primaryBuffer.Add(enumerator.Current);
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }

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

    private static bool IsRetriable(Exception ex)
    {
        // HTTP errors: 429 (rate-limit) and 5xx (server errors)
        if (ex is HttpRequestException httpEx)
        {
            if (httpEx.StatusCode is null) return true;
            var status = (int)httpEx.StatusCode;
            return status == 429 || status >= 500;
        }

        // Catch OpenAI SDK exceptions (ClientResultException)
        if (ex.GetType().Name == "ClientResultException")
        {
            var status = (int?)ex.GetType().GetProperty("Status")?.GetValue(ex) ?? 0;
            return status == 429 || status >= 500 || ex.Message.Contains("429") || ex.Message.Contains("rate_limit_exceeded");
        }

        // Timeouts (TaskCanceledException wraps OperationCanceledException for HttpClient timeouts)
        if (ex is TaskCanceledException)
            return true;

        // Connection failures
        if (ex is SocketException)
            return true;

        // Broken pipe / stream read failures
        if (ex is IOException)
            return true;

        return false;
    }
}
