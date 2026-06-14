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

namespace LetopiaPlatform.IntegrationTests.Infrastructure;

/// <summary>
/// Shared API host for integration tests: in-memory database, test JWT validation, stub roadmap agent.
/// </summary>
public class AgentIntegrationWebApplicationFactory : WebApplicationFactory<API.Program>
{
    public const string TestSecretKey = "ThisIsATestSecretKeyForIntegrationTests_MustBe256Bits!!";
    public const string TestIssuer = "test-issuer";
    public const string TestAudience = "test-audience";

    private readonly string _dbName = $"IntegrationTests_{Guid.NewGuid()}";

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureWebHost(webBuilder =>
        {
            webBuilder.UseEnvironment("Testing");
        });

        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>))
                .ToList();
            foreach (var d in descriptors)
                services.Remove(d);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(TestSecretKey));
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

            services.RemoveAll<IRoadmapAgentService>();
            services.AddScoped<IRoadmapAgentService, StubRoadmapAgentService>();
        });

        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();

        return host;
    }

    internal sealed class StubRoadmapAgentService : IRoadmapAgentService
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
            Guid conversationId, string userMessage, Guid userId, bool saveUserMessage = true,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            await Task.CompletedTask;
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
