using System.Text.Json;
using LetopiaPlatform.Agent.Services;
using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Exceptions;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Moq;

namespace LetopiaPlatform.UnitTests.Agent;

public class RoadmapAgentServiceTests
{
    private static readonly SearchResult[] SingleSearchResult =
        [new SearchResult("Title", "https://example.com", "Snippet")];

    private static readonly string[] SingleInsight = ["Start with syntax"];
    private static readonly string[] UpdatedInsight = ["Updated insight"];

    private readonly Mock<IChatClient> _mockChatClient = new();
    private readonly Mock<IWebSearchService> _mockWebSearch = new();
    private readonly Mock<IRoadmapRepository> _mockRoadmapRepo = new();
    private readonly Mock<IConversationRepository> _mockConversationRepo = new();
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly RoadmapAgentService _service;

    public RoadmapAgentServiceTests()
    {
        _service = new RoadmapAgentService(
            _mockChatClient.Object,
            _mockWebSearch.Object,
            _mockRoadmapRepo.Object,
            _mockConversationRepo.Object,
            _mockUnitOfWork.Object,
            Mock.Of<ILogger<RoadmapAgentService>>());
    }

    [Fact]
    public async Task StartConversationCreatesConversationAndMessages()
    {
        var userId = Guid.NewGuid();
        var message = "I want to learn C#";

        AgentConversation? captured = null;
        var capturedMessages = new List<ConversationMessage>();

        _mockConversationRepo
            .Setup(r => r.Add(It.IsAny<AgentConversation>()))
            .Callback<AgentConversation>(c => captured = c);

        _mockConversationRepo
            .Setup(r => r.AddMessage(It.IsAny<ConversationMessage>()))
            .Callback<ConversationMessage>(m => capturedMessages.Add(m));

        var result = await _service.StartConversationAsync(userId, message, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(userId, captured.UserId);
        Assert.Equal(AgentType.RoadmapGenerator, captured.AgentType);
        Assert.Equal(ConversationStatus.Active, captured.Status);
        Assert.Equal(message, captured.Title);

        Assert.Equal(2, capturedMessages.Count);
        Assert.Equal(MessageRole.System, capturedMessages[0].Role);
        Assert.Equal(MessageRole.User, capturedMessages[1].Role);
        Assert.Equal(message, capturedMessages[1].Content);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMessageTextResponseYieldsDeltaEvents()
    {
        var conversationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupConversation(conversationId, userId);
        SetupStreamingTextResponse("Here is some advice for your learning journey.");

        var events = await CollectEvents(conversationId, "help me learn", userId);

        Assert.Contains(events, e => e.Type == "delta");
        Assert.Contains(events, e => e.Type == "done");
    }

    [Fact]
    public async Task ProcessMessageToolCallExecutesToolAndContinues()
    {
        var conversationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupConversation(conversationId, userId);

        var callSequence = 0;
        _mockChatClient
            .Setup(c => c.CompleteStreamingAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callSequence++;
                if (callSequence == 1)
                {
                    return ToAsyncEnumerable(new StreamingChatCompletionUpdate
                    {
                        Contents = [new FunctionCallContent("call_1", "search_web",
                            new Dictionary<string, object?> { ["query"] = "C# tutorials" })]
                    });
                }

                return ToAsyncEnumerable(new StreamingChatCompletionUpdate
                {
                    Contents = [new TextContent("Here are some resources.")]
                });
            });

        _mockWebSearch
            .Setup(s => s.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SingleSearchResult.ToList());

        var events = await CollectEvents(conversationId, "find resources", userId);

        Assert.Contains(events, e => e.Type == "status");
        Assert.Contains(events, e => e.Type == "delta");
        Assert.Contains(events, e => e.Type == "done");
        Assert.Equal(2, callSequence);

        // Verify assistant tool-call message and tool result persisted to DB
        _mockConversationRepo.Verify(
            r => r.AddMessage(It.Is<ConversationMessage>(m => m.Role == MessageRole.Assistant)),
            Times.AtLeastOnce);
        _mockConversationRepo.Verify(
            r => r.AddMessage(It.Is<ConversationMessage>(m => m.Role == MessageRole.Tool)),
            Times.Once);
        // SaveChangesAsync at least twice: tool-call persist + final assistant message
        _mockUnitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.AtLeast(2));
    }

    [Fact]
    public async Task ProcessMessageRoadmapJsonSavesAndYieldsComplete()
    {
        var conversationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupConversation(conversationId, userId);

        var roadmapJson = JsonSerializer.Serialize(new
        {
            title = "Learn C#",
            topic = "C# Programming",
            description = "A roadmap for learning C#",
            estimatedDurationWeeks = 12,
            phases = new[]
            {
                new
                {
                    title = "Basics",
                    description = "Learn fundamentals",
                    order = 1,
                    durationEstimateWeeks = 4,
                    resources = Array.Empty<object>(),
                    projects = Array.Empty<object>(),
                    insights = SingleInsight
                }
            }
        });

        SetupStreamingTextResponse($"StartOfAnswer\n{roadmapJson}\nEndOfAnswer");

        var events = await CollectEvents(conversationId, "generate roadmap", userId);

        Assert.Contains(events, e => e.Type == "roadmap_complete");
        _mockRoadmapRepo.Verify(r => r.Add(It.IsAny<Roadmap>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMessagePhaseEditUpdatesAndYieldsPhaseUpdated()
    {
        var conversationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roadmapId = Guid.NewGuid();
        var phaseId = Guid.NewGuid();

        SetupConversation(conversationId, userId, roadmapId);

        var phase = new RoadmapPhase
        {
            Id = phaseId,
            RoadmapId = roadmapId,
            Title = "Old Title",
            Description = "Old Desc",
            Order = 1,
            DurationEstimateWeeks = 2
        };

        _mockRoadmapRepo
            .Setup(r => r.GetPhaseByIdAsync(phaseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(phase);

        var phaseJson = JsonSerializer.Serialize(new
        {
            phaseId,
            title = "Updated Phase",
            description = "Updated description",
            order = 1,
            durationEstimateWeeks = 3,
            resources = Array.Empty<object>(),
            projects = Array.Empty<object>(),
            insights = UpdatedInsight
        });

        SetupStreamingTextResponse($"StartOfAnswer\n{phaseJson}\nEndOfAnswer");

        var events = await CollectEvents(conversationId, "update this phase", userId);

        Assert.Contains(events, e => e.Type == "phase_updated");
        _mockRoadmapRepo.Verify(r => r.GetPhaseByIdAsync(phaseId, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("Updated Phase", phase.Title);
    }

    [Fact]
    public async Task ProcessMessageWrongUserThrowsForbidden()
    {
        var conversationId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var wrongUserId = Guid.NewGuid();
        SetupConversation(conversationId, ownerId);

        await Assert.ThrowsAsync<ForbiddenException>(async () =>
        {
            await foreach (var _ in _service.ProcessMessageAsync(
                conversationId, "hello", wrongUserId, CancellationToken.None))
            {
            }
        });
    }

    [Fact]
    public async Task ProcessMessageMaxIterationsYieldsError()
    {
        var conversationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupConversation(conversationId, userId);

        _mockChatClient
            .Setup(c => c.CompleteStreamingAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .Returns(() => ToAsyncEnumerable(new StreamingChatCompletionUpdate
            {
                Contents = [new FunctionCallContent("call_1", "search_web",
                    new Dictionary<string, object?> { ["query"] = "test" })]
            }));

        _mockWebSearch
            .Setup(s => s.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SingleSearchResult.ToList());

        var events = await CollectEvents(conversationId, "go", userId);

        var errorEvent = events.Last();
        Assert.Equal("error", errorEvent.Type);
        Assert.Contains("Max iterations reached", errorEvent.Data?.ToString());
    }

    #region Helpers

    private AgentConversation SetupConversation(Guid conversationId, Guid userId, Guid? roadmapId = null)
    {
        var conversation = new AgentConversation
        {
            Id = conversationId,
            UserId = userId,
            RoadmapId = roadmapId,
            AgentType = AgentType.RoadmapGenerator,
            Status = ConversationStatus.Active,
            Title = "Test",
            Messages = new List<ConversationMessage>
            {
                new()
                {
                    ConversationId = conversationId,
                    Role = MessageRole.System,
                    Content = "system prompt",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-2)
                }
            }
        };

        _mockConversationRepo
            .Setup(r => r.GetByIdWithMessagesAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        return conversation;
    }

    private void SetupStreamingTextResponse(string text)
    {
        _mockChatClient
            .Setup(c => c.CompleteStreamingAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .Returns(() => ToAsyncEnumerable(new StreamingChatCompletionUpdate
            {
                Contents = [new TextContent(text)]
            }));
    }

    private async Task<List<AgentStreamEvent>> CollectEvents(
        Guid conversationId, string message, Guid userId)
    {
        var events = new List<AgentStreamEvent>();
        await foreach (var e in _service.ProcessMessageAsync(
            conversationId, message, userId, CancellationToken.None))
        {
            events.Add(e);
        }

        return events;
    }

    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(params T[] items)
    {
        foreach (var item in items)
        {
            yield return item;
            await Task.CompletedTask;
        }
    }

    #endregion
}
