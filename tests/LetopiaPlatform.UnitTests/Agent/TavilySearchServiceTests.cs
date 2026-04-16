using System.Net;
using System.Text;
using System.Text.Json;
using LetopiaPlatform.Agent.Configuration;
using LetopiaPlatform.Agent.Services;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace LetopiaPlatform.UnitTests.Agent;

/// <summary>
/// Unit tests for <see cref="TavilySearchService"/>.
/// Uses a mocked <see cref="HttpMessageHandler"/> to verify request format and response mapping.
/// </summary>
public class TavilySearchServiceTests
{
    private const string FakeApiKey = "tvly-test-key-123";

    private static TavilySearchService CreateService(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var settings = Options.Create(new WebSearchSettings { TavilyApiKey = FakeApiKey });
        var logger = Mock.Of<ILogger<TavilySearchService>>();
        return new TavilySearchService(httpClient, settings, logger);
    }

    /// <summary>
    /// Verifies that valid Tavily API JSON is correctly mapped to <see cref="SearchResult"/> records
    /// and that the outgoing request body contains the expected fields.
    /// </summary>
    [Fact]
    public async Task SearchAsyncReturnsResultsWhenApiReturnsValidJson()
    {
        // Arrange
        var tavilyJson = JsonSerializer.Serialize(new
        {
            results = new[]
            {
                new { title = "C# Tutorial", url = "https://example.com/csharp", content = "Learn C# basics" },
                new { title = "ASP.NET Guide", url = "https://example.com/aspnet", content = "Build web apps" }
            }
        });

        string? capturedRequestBody = null;
        Uri? capturedUri = null;

        var handler = new MockHttpMessageHandler(async (request, _) =>
        {
            capturedUri = request.RequestUri;
            capturedRequestBody = await request.Content!.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(false);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(tavilyJson, Encoding.UTF8, "application/json")
            };
        });

        var service = CreateService(handler);

        // Act
        var results = await service.SearchAsync("best C# tutorials", maxResults: 2);

        // Assert — response mapping
        Assert.Equal(2, results.Count);
        Assert.Equal("C# Tutorial", results[0].Title);
        Assert.Equal("https://example.com/csharp", results[0].Url);
        Assert.Equal("Learn C# basics", results[0].Snippet);
        Assert.Equal("ASP.NET Guide", results[1].Title);

        // Assert — request format
        Assert.Equal(new Uri("https://api.tavily.com/search"), capturedUri);
        Assert.NotNull(capturedRequestBody);

        using var doc = JsonDocument.Parse(capturedRequestBody);
        var root = doc.RootElement;
        Assert.Equal("best C# tutorials", root.GetProperty("query").GetString());
        Assert.Equal(2, root.GetProperty("max_results").GetInt32());
        Assert.Equal(FakeApiKey, root.GetProperty("api_key").GetString());
    }

    /// <summary>
    /// Verifies that an HTTP error response results in an empty list rather than an exception.
    /// </summary>
    [Fact]
    public async Task SearchAsyncReturnsEmptyListWhenApiReturnsError()
    {
        // Arrange
        var handler = new MockHttpMessageHandler((_, _) =>
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Internal Server Error")
            });
        });

        var service = CreateService(handler);

        // Act
        var results = await service.SearchAsync("test query");

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    /// <summary>
    /// Verifies that a network-level failure results in an empty list rather than an exception.
    /// </summary>
    [Fact]
    public async Task SearchAsyncReturnsEmptyListWhenNetworkFails()
    {
        // Arrange
        var handler = new MockHttpMessageHandler((_, _) =>
            throw new HttpRequestException("Network unreachable"));

        var service = CreateService(handler);

        // Act
        var results = await service.SearchAsync("test query");

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    /// <summary>
    /// Verifies that the <c>max_results</c> field in the request body matches
    /// the <paramref name="maxResults"/> parameter passed to <see cref="IWebSearchService.SearchAsync"/>.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(10)]
    public async Task SearchAsyncRespectsMaxResults(int maxResults)
    {
        // Arrange
        string? capturedBody = null;

        var handler = new MockHttpMessageHandler(async (request, _) =>
        {
            capturedBody = await request.Content!.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(false);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"results\":[]}", Encoding.UTF8, "application/json")
            };
        });

        var service = CreateService(handler);

        // Act
        await service.SearchAsync("query", maxResults: maxResults);

        // Assert
        Assert.NotNull(capturedBody);
        using var doc = JsonDocument.Parse(capturedBody);
        Assert.Equal(maxResults, doc.RootElement.GetProperty("max_results").GetInt32());
    }

    #region Test Helpers

    /// <summary>
    /// A test double for <see cref="HttpMessageHandler"/> that delegates to a provided function.
    /// </summary>
    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

        public MockHttpMessageHandler(
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
        {
            _sendAsync = sendAsync;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _sendAsync(request, cancellationToken);
        }
    }

    #endregion
}
