using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using LetopiaPlatform.API.Common;
using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace LetopiaPlatform.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for <see cref="API.Controllers.RoadmapsController"/>.
/// </summary>
[Collection(IntegrationApiCollection.Name)]
public class RoadmapsControllerTests
{
    private readonly HttpClient _client;

    public RoadmapsControllerTests(AgentIntegrationWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task ListRoadmapsWithJwtReturns200WithEmptyList()
    {
        var userId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateTestJwt(userId));

        var response = await _client.GetAsync("/api/v1/roadmaps");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<RoadmapSummaryDto>>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.Empty(body.Data);
    }

    [Fact]
    public async Task GetRoadmapByNonExistentIdReturns404()
    {
        var userId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateTestJwt(userId));

        var response = await _client.GetAsync($"/api/v1/roadmaps/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PatchPhaseStatusForNonExistentRoadmapReturns404()
    {
        var userId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateTestJwt(userId));

        var roadmapId = Guid.NewGuid();
        var phaseId = Guid.NewGuid();
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/roadmaps/{roadmapId}/phases/{phaseId}/status",
            new UpdatePhaseStatusRequest(PhaseStatus.Completed));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListRoadmapsWithoutAuthReturns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/v1/roadmaps");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetRoadmapWithoutAuthReturns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync($"/api/v1/roadmaps/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PatchPhaseStatusWithoutAuthReturns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/roadmaps/{Guid.NewGuid()}/phases/{Guid.NewGuid()}/status",
            new UpdatePhaseStatusRequest(PhaseStatus.Completed));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
