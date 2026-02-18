using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.ProjectCategory.Response;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
namespace LetopiaPlatform.Infrastructure.Services;

public class ProjectCategoryService : IProjectCategoryService
{
    private readonly IProjectCategoryRepository _projectCategoryRepository;
    private readonly ILogger<ProjectCategoryService> _logger;
    public ProjectCategoryService(IProjectCategoryRepository projectCategoryRepository, ILogger<ProjectCategoryService> logger)
    {
        _projectCategoryRepository = projectCategoryRepository;
        _logger = logger;

    }

    public async Task<Result<IEnumerable<CategoryResponse>>> GetAllOrderedAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all ordered categories");
        var categories = await _projectCategoryRepository.GetOrderedCategoriesAsync(ct);
        return Result<IEnumerable<CategoryResponse>>.Success(categories.Select(MapToResponse));
    }

    public async Task<Result<CategoryResponse>> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching category details for slug: {Slug}", slug);
        var category = await _projectCategoryRepository.GetCategoryWithProjectsAsync(slug, ct);

        if (category is null)
        {
            _logger.LogWarning("Category with slug {Slug} not found", slug);
            return Result<CategoryResponse>.Failure("Category not found", 404);
        }

        return Result<CategoryResponse>.Success(MapToResponse(category));
    }

    public async Task<Result<Dictionary<Guid, int>>> GetCategoryStatsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching project counts per category");
        var stats = await _projectCategoryRepository.GetCategoryProjectCountsAsync(ct);
        return Result<Dictionary<Guid, int>>.Success(stats);
    }

    public async Task<Result<bool>> DeleteCategoryAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation("Attempting to delete category {CategoryId}", id);
        var category = await _projectCategoryRepository.GetByIdAsync(id);

        if (category is null) return Result<bool>.Failure("Category not found", 404);

        if (await _projectCategoryRepository.HasProjectsAsync(id, ct))
        {
            _logger.LogWarning("Delete failed: Category {CategoryId} has projects", id);
            return Result<bool>.Failure("Cannot delete category containing projects", 400);
        }

        await _projectCategoryRepository.DeleteAsync(category); // تأكد أن GenericRepo يدعم ct لو أردت
        return Result<bool>.Success(true);
    }

    private static CategoryResponse MapToResponse(ProjectCategory category) => new(
        Id: category.Id,
        Name: category.Name,
        Slug: category.Slug,
        IconUrl: category.IconUrl,
        DisplayOrder: category.DisplayOrder,
        Projects: category.Projects?.Select(p => new ProjectSummaryResponse(p.Id, p.Title)).ToList()
                  ?? new List<ProjectSummaryResponse>()
    );
}

