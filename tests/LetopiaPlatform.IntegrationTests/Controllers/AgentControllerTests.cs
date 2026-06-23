using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using LetopiaPlatform.API.Common;
using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Infrastructure.Data;
using LetopiaPlatform.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace LetopiaPlatform.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for the AgentController.
/// Verifies SSE content-type header on POST with JWT, and basic endpoint behaviour.
/// </summary>
[Collection(IntegrationApiCollection.Name)]
public class AgentControllerTests
{
    private readonly HttpClient _client;
    private readonly AgentIntegrationWebApplicationFactory _factory;

    public AgentControllerTests(AgentIntegrationWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task StartConversationWithJwtReturnsSseContentType()
    {
        var userId = Guid.NewGuid();
        var token = GenerateTestJwt(userId);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new StartConversationRequest("I want to learn C#");

        var response = await _client.PostAsJsonAsync("/api/v1/agent/conversations", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/event-stream", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ListConversationsWithJwtReturns200WithEmptyListForNewUser()
    {
        var userId = Guid.NewGuid();
        var token = GenerateTestJwt(userId);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/agent/conversations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ConversationSummaryDto>>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.Empty(body.Data);
    }

    [Fact]
    public async Task SendMessageToNonExistentConversationReturns404()
    {
        var userId = Guid.NewGuid();
        var token = GenerateTestJwt(userId);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var missingId = Guid.NewGuid();
        var request = new SendMessageRequest("Hello");

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/agent/conversations/{missingId}/messages", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task StartConversationWithoutJwtReturns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var request = new StartConversationRequest("I want to learn C#");

        var response = await _client.PostAsJsonAsync("/api/v1/agent/conversations", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SendMessageWithJwtOwnedConversationReturnsSseContentType()
    {
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

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/agent/conversations/{conversationId}/messages", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/event-stream", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task SendMessageToOtherUsersConversationReturns403()
    {
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

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/agent/conversations/{conversationId}/messages", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static string GenerateTestJwt(Guid userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AgentIntegrationWebApplicationFactory.TestSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: AgentIntegrationWebApplicationFactory.TestIssuer,
            audience: AgentIntegrationWebApplicationFactory.TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
