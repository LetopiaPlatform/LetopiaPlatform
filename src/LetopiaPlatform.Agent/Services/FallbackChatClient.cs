using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
        try
        {
            _logger.LogDebug("Attempting CompleteAsync with primary provider.");
            var result = await base.CompleteAsync(chatMessages, options, cancellationToken);
            _logger.LogDebug("Primary provider CompleteAsync succeeded.");
            return result;
        }
        catch (Exception ex) when (IsRetriable(ex))
        {
            _logger.LogWarning(ex,
                "Primary provider CompleteAsync failed with retriable error. Falling back to secondary provider.");
            return await _fallback.CompleteAsync(chatMessages, options, cancellationToken);
        }
    }

    public override async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Buffer primary deltas internally to avoid partial-content duplication
        // when the primary provider fails mid-stream.
        var primaryBuffer = new List<StreamingChatCompletionUpdate>();
        bool useFallback = false;

        var enumerator = base.CompleteStreamingAsync(chatMessages, options, cancellationToken)
            .GetAsyncEnumerator(cancellationToken);

        _logger.LogDebug("Attempting CompleteStreamingAsync with primary provider.");

        try
        {
            while (true)
            {
                bool moved;
                try
                {
                    moved = await enumerator.MoveNextAsync();
                }
                catch (Exception ex) when (IsRetriable(ex))
                {
                    _logger.LogWarning(ex,
                        "Primary provider failed mid-stream after {BufferedChunks} chunk(s). "
                        + "Discarding buffered deltas and falling back to secondary provider.",
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
