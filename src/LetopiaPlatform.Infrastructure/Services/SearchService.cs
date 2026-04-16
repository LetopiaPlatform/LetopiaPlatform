using LetopiaPlatform.Core.DTOs.Community;
using LetopiaPlatform.Core.DTOs.Search;
using LetopiaPlatform.Core.DTOs.User;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Infrastructure.Services;

public class SearchService : ISearchService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SearchService> _logger;

    public SearchService(ApplicationDbContext dbContext, ILogger<SearchService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<GlobalSearchResultDto> SearchAsync(
        string query,
        string? type = null,
        int limit = 5,
        CancellationToken ct = default)
    {
        var pattern = $"%{query.Trim()}%";

        var communities = new List<CommunitySummaryDto>();
        var projects = new List<ProjectSearchResultDto>();
        var members = new List<UserSummaryDto>();

        // Search communities
        if (type is null or "communities")
        {
            communities = await _dbContext.Communities
                .AsNoTracking()
                .Where(c => c.IsActive)
                .Where(c =>
                    EF.Functions.ILike(c.Name, pattern) ||
                    EF.Functions.ILike(c.Description, pattern))
                .OrderByDescending(c => c.MemberCount)
                .Take(limit)
                .Select(c => new CommunitySummaryDto(
                    c.Id, c.Name, c.Slug, c.Description,
                    c.CategoryId, c.Category.ParentCategory != null
                        ? c.Category.ParentCategory.Name : c.Category.Name,
                    c.Category.ParentCategory != null
                        ? c.Category.ParentCategory.IconUrl : c.Category.IconUrl,
                    c.CoverImageUrl,
                    c.MemberCount,
                    c.PostCount,
                    c.IsPrivate,
                    c.CreatedAt,
                    c.Category.Name,
                    c.Category.Slug,
                    c.Category.ParentCategoryId ?? c.CategoryId))
                .ToListAsync(ct);
        }

        // Search projects
        if (type is null or "projects")
        {
            projects = await _dbContext.Projects
                .AsNoTracking()
                .Where(p =>
                    EF.Functions.ILike(p.Title, pattern) ||
                    EF.Functions.ILike(p.Description, pattern))
                .OrderByDescending(p => p.CreatedAt)
                .Take(limit)
                .Select(p => new ProjectSearchResultDto(
                    p.Id,
                    p.Title,
                    p.Description,
                    p.CoverImageUrl,
                    p.Category.Name,
                    p.DifficultyLevel.ToString(),
                    p.Status.ToString()))
                .ToListAsync(ct);
        }

        // Search members
        if (type is null or "members")
        {
            members = await _dbContext.Users
                .AsNoTracking()
                .Where(u =>
                    (u.FullName != null && EF.Functions.ILike(u.FullName, pattern)) ||
                    (u.UserName != null && EF.Functions.ILike(u.UserName, pattern)))
                .OrderBy(u => u.FullName)
                .Take(limit)
                .Select(u => new UserSummaryDto(
                    u.Id,
                    u.FullName ?? u.UserName ?? string.Empty,
                    u.UserName ?? string.Empty,
                    u.AvatarUrl))
                .ToListAsync(ct);
        }

        _logger.LogInformation(
            "Search completed: query='{Query}', type={Type}, results: {Communities}C/{Projects}P/{Members}M",
            query, type ?? "all",
            communities.Count, projects.Count, members.Count);

        return new GlobalSearchResultDto(communities, projects, members);
    }
}
