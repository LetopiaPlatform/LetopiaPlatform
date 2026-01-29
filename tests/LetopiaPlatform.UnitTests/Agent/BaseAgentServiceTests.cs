using LetopiaPlatform.Agent.Abstractions;
using LetopiaPlatform.Agent.Configuration;
using LetopiaPlatform.Agent.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OpenAI.Chat;
using Xunit;

namespace LetopiaPlatform.UnitTests.Agent;

public class TestAgentService : BaseAgentService
{
    public TestAgentService(
        IOptions<AgentSettings> options,
        ILogger logger,
        IConversationCache conversationCache)
        : base(options, logger, conversationCache)
    {
    }

    public override string AgentType => "TestAgent";

    protected override string GetSystemPrompt() => "You are a test agent.";

    protected override IEnumerable<ChatTool> GetTools() => Enumerable.Empty<ChatTool>();
}

public class BaseAgentServiceTests
{
    private readonly Mock<IOptions<AgentSettings>> _mockOptions;
    private readonly Mock<ILogger<TestAgentService>> _mockLogger;
    private readonly Mock<IConversationCache> _mockCache;
    private readonly AgentSettings _settings;

    public BaseAgentServiceTests()
    {
        _settings = new AgentSettings
        {
            GitHubToken = "fake-token",
            GitHubModelsEndpoint = "https://models.github.ai/inference",
            ModelId = "gpt-4o"
        };

        _mockOptions = new Mock<IOptions<AgentSettings>>();
        _mockOptions.Setup(o => o.Value).Returns(_settings);

        _mockLogger = new Mock<ILogger<TestAgentService>>();
        _mockCache = new Mock<IConversationCache>();
    }

    [Fact]
    public void CreateNewThread_ReturnsCorrectFormat()
    {
        // Arrange
        var service = new TestAgentService(_mockOptions.Object, _mockLogger.Object, _mockCache.Object);

        // Act
        var threadId = service.CreateNewThread();

        // Assert
        Assert.NotNull(threadId);
        Assert.StartsWith("TestAgent:", threadId);
        Assert.True(Guid.TryParse(threadId.Split(':')[1], out _));
    }

    [Fact]
    public async Task RunAsync_WithInvalidToken_ThrowsClientResultException()
    {
        // Arrange
        // Using the fake token from setup
        var service = new TestAgentService(_mockOptions.Object, _mockLogger.Object, _mockCache.Object);
        var threadId = service.CreateNewThread();

        // Act & Assert
        await Assert.ThrowsAsync<System.ClientModel.ClientResultException>(async () => 
        {
            await service.RunAsync("Hello AI", threadId);
        });
    }

}
