using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace LetopiaPlatform.Agent.Services;

/// <summary>
/// A delegating <see cref="IChatClient"/> that tries the primary provider first
/// and falls back to a secondary on retriable errors (429, 5xx, network failures).
/// Does not fall back on 400 (Bad Request) errors.
/// </summary>
public sealed class FallbackChatClient : DelegatingChatClient
{
    private readonly IChatClient _fallback;

    public FallbackChatClient(IChatClient primary, IChatClient fallback)
        : base(primary)
    {
        _fallback = fallback;
    }

    public override async Task<ChatCompletion> CompleteAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.CompleteAsync(chatMessages, options, cancellationToken);
        }
        catch (Exception ex) when (IsRetriable(ex))
        {
            return await _fallback.CompleteAsync(chatMessages, options, cancellationToken);
        }
    }

    public override async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var enumerator = base.CompleteStreamingAsync(chatMessages, options, cancellationToken)
            .GetAsyncEnumerator(cancellationToken);
        bool useFallback = false;

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
                    useFallback = true;
                    break;
                }

                if (!moved) break;
                yield return enumerator.Current;
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }

        if (useFallback)
        {
            await foreach (var update in _fallback.CompleteStreamingAsync(chatMessages, options, cancellationToken))
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
        if (ex is HttpRequestException httpEx)
        {
            if (httpEx.StatusCode is null) return true;
            var status = (int)httpEx.StatusCode;
            return status == 429 || status >= 500;
        }

        return false;
    }
}
