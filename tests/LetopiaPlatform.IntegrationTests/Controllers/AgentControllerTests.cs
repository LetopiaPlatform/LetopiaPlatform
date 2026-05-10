using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace LetopiaPlatform.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for the AgentController.
/// Verifies SSE content-type header on POST with JWT, and basic endpoint behaviour.
/// </summary>
public class AgentControllerTests : IClassFixture<AgentControllerTests.AgentApiFactory>
{
    private const string TestSecretKey = "ThisIsATestSecretKeyForIntegrationTests_MustBe256Bits!!";
    private const string TestIssuer = "test-issuer";
    private const string TestAudience = "test-audience";

    private readonly HttpClient _client;
    private readonly AgentApiFactory _factory;

    public AgentControllerTests(AgentApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            // Don't follow redirects — we need raw SSE response
            AllowAutoRedirect = false
        });
    }

    /// <summary>
    /// POST /api/v1/agent/conversations with a valid JWT
    /// → verify response has content-type: text/event-stream.
    /// </summary>
    [Fact]
    public async Task StartConversationWithJwtReturnsSseContentType()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = GenerateTestJwt(userId);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new StartConversationRequest("I want to learn C#");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/agent/conversations", request);

        // Assert — the response should be SSE (text/event-stream)
        Assert.Equal("text/event-stream", response.Content.Headers.ContentType?.MediaType);
    }

    /// <summary>
    /// POST /api/v1/agent/conversations without JWT → 401 Unauthorized.
    /// </summary>
    [Fact]
    public async Task StartConversationWithoutJwtReturns401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;
        var request = new StartConversationRequest("I want to learn C#");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/agent/conversations", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// POST /api/v1/agent/conversations/{id}/messages with a valid JWT
    /// for a conversation that belongs to the user → SSE content-type.
    /// </summary>
    [Fact]
    public async Task SendMessageWithJwtOwnedConversationReturnsSseContentType()
    {
        // Arrange — seed a conversation
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.AgentConversations.Add(new AgentConversation
            {
                Id = conversationId,
                UserId = userId,
                AgentType = AgentType.RoadmapGenerator,
                Status = ConversationStatus.Active,
                Title = "Test Conversation"
            });
            await db.SaveChangesAsync();
        }

        var token = GenerateTestJwt(userId);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new SendMessageRequest("Tell me more about roadmaps");

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/agent/conversations/{conversationId}/messages", request);

        // Assert
        Assert.Equal("text/event-stream", response.Content.Headers.ContentType?.MediaType);
    }

    /// <summary>
    /// POST /api/v1/agent/conversations/{id}/messages for a conversation
    /// belonging to another user → 403 Forbidden.
    /// </summary>
    [Fact]
    public async Task SendMessageToOtherUsersConversationReturns403()
    {
        // Arrange — seed a conversation owned by another user
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.AgentConversations.Add(new AgentConversation
            {
                Id = conversationId,
                UserId = ownerId,
                AgentType = AgentType.RoadmapGenerator,
                Status = ConversationStatus.Active,
                Title = "Other User's Conversation"
            });
            await db.SaveChangesAsync();
        }

        var token = GenerateTestJwt(attackerId);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new SendMessageRequest("Trying to access");

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/agent/conversations/{conversationId}/messages", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Generates a JWT token matching the test authentication scheme.
    /// </summary>
    private static string GenerateTestJwt(Guid userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ───────────────────────────────────────────────────────────
    //  Custom WebApplicationFactory
    // ───────────────────────────────────────────────────────────

    /// <summary>
    /// Configures the test server with an in-memory DB, overridden JWT auth,
    /// and a stub agent service that immediately completes the SSE stream.
    /// Bypasses Program.Main() entirely by building a standalone host.
    /// </summary>
    public class AgentApiFactory : WebApplicationFactory<API.Program>
    {
        private readonly string _dbName = $"AgentTests_{Guid.NewGuid()}";

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseEnvironment("Testing");
            });

            builder.ConfigureServices(services =>
            {
                // ── Replace the real DB with an in-memory one ──────────
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>))
                    .ToList();
                foreach (var d in descriptors)
                    services.Remove(d);

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));

                // ── Replace JWT auth to accept our test tokens ─────────
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        IssuerSigningKey = key,
                        ValidIssuer = TestIssuer,
                        ValidAudience = TestAudience,
                        ClockSkew = TimeSpan.Zero
                    };
                });

                // ── Replace the agent service with a stub ──────────────
                services.RemoveAll<IRoadmapAgentService>();
                services.AddScoped<IRoadmapAgentService, StubRoadmapAgentService>();
            });

            var host = base.CreateHost(builder);

            // Ensure the in-memory DB is created
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();

            return host;
        }
    }

    // ───────────────────────────────────────────────────────────
    //  Stub Agent Service
    // ───────────────────────────────────────────────────────────

    /// <summary>
    /// Minimal stub that creates a real conversation in the DB
    /// and yields a single "done" event for SSE streaming tests.
    /// </summary>
    private sealed class StubRoadmapAgentService : IRoadmapAgentService
    {
        private readonly IConversationRepository _repo;
        private readonly IUnitOfWork _unitOfWork;

        public StubRoadmapAgentService(IConversationRepository repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        public AgentType AgentType => AgentType.RoadmapGenerator;

        public async Task<AgentConversation> StartConversationAsync(
            Guid userId, string initialMessage, CancellationToken ct)
        {
            var conversation = new AgentConversation
            {
                UserId = userId,
                AgentType = AgentType.RoadmapGenerator,
                Status = ConversationStatus.Active,
                Title = initialMessage.Length > 100 ? initialMessage[..100] : initialMessage
            };

            _repo.Add(conversation);
            await _unitOfWork.SaveChangesAsync(ct);
            return conversation;
        }

        public async IAsyncEnumerable<AgentStreamEvent> ProcessMessageAsync(
            Guid conversationId, string userMessage, Guid userId,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
        {
            await Task.CompletedTask; // keep async signature valid
            yield return new AgentStreamEvent("delta", "Hello from stub");
            yield return new AgentStreamEvent("done", null);
        }

        public async Task ValidateConversationOwnershipAsync(Guid conversationId, Guid userId, CancellationToken ct)
        {
            var conversation = await _repo.GetByIdAsync(conversationId, ct)
                ?? throw new Core.Exceptions.NotFoundException(nameof(AgentConversation), conversationId);

            if (conversation.UserId != userId)
                throw new Core.Exceptions.ForbiddenException();
        }

        public async Task<List<ConversationSummaryDto>> GetUserConversationsAsync(Guid userId, CancellationToken ct)
        {
            var conversations = await _repo.GetByUserIdAsync(userId, ct);

            return conversations
                .OrderByDescending(c => c.UpdatedAt)
                .Select(c => new ConversationSummaryDto(
                    c.Id, c.Title, c.AgentType, c.Status, c.CreatedAt, c.UpdatedAt))
                .ToList();
        }

        public async Task<ConversationDto> GetConversationAsync(Guid conversationId, Guid userId, CancellationToken ct)
        {
            var conversation = await _repo.GetByIdWithMessagesAsync(conversationId, ct)
                ?? throw new Core.Exceptions.NotFoundException(nameof(AgentConversation), conversationId);

            if (conversation.UserId != userId)
                throw new Core.Exceptions.ForbiddenException();

            return new ConversationDto(
                conversation.Id, conversation.Title, conversation.AgentType,
                conversation.Status, conversation.RoadmapId,
                conversation.CreatedAt, conversation.UpdatedAt,
                (conversation.Messages ?? [])
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new ConversationMessageDto(m.Id, m.Role, m.Content, m.CreatedAt))
                    .ToList());
        }
    }
}
