using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.ProjectCategory.Request;
using LetopiaPlatform.Core.DTOs.ProjectCategory.Response;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
namespace LetopiaPlatform.Infrastructure.Services;

public class ProjectCategoryService : IProjectCategoryService
{
    private readonly IProjectCategoryRepository _projectCategoryRepository;
    private readonly ILogger<ProjectCategoryService> _logger;
    private readonly IFileStorageService _fileService;
    public ProjectCategoryService(IProjectCategoryRepository projectCategoryRepository, ILogger<ProjectCategoryService> logger, IFileStorageService fileStorage)
    {
        _projectCategoryRepository = projectCategoryRepository;
        _logger = logger;
        _fileService = fileStorage;
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
    // ── Create Category ──────────────────────────────────────────────────────
    public async Task<Result<Guid>> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating new category: {CategoryName}", request.Name);

        // التأكد إن الـ Slug مش مستخدم قبل كدة
        if (await _projectCategoryRepository.SlugExistsAsync(request.Slug, null, ct))
        {
            return Result<Guid>.Failure("This slug is already in use", 400);
        }
        //IConUrl
        string? iconUrl = null;
        if (request.IconUrl is not null)
        {
            var uploadResult = await _fileService.UploadAsync(request.IconUrl, "categories");

            // تأكد من نجاح عملية الرفع
            if (!uploadResult.IsSuccess)
            {
                return Result<Guid>.Failure("uploadIconUrlIsFail", uploadResult.StatusCode);
            }

            iconUrl = uploadResult.Value; // هنا بناخد الـ string المباشر من الـ Result
        }


        var category = new ProjectCategory
        {
            Name = request.Name,
            Slug = request.Slug,
            IconUrl = iconUrl,
            DisplayOrder = request.DisplayOrder
        };

        await _projectCategoryRepository.AddAsync(category);
        return Result<Guid>.Success(category.Id);
    }

    // ── Update Category ──────────────────────────────────────────────────────
    public async Task<Result<bool>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating category ID: {CategoryId}", id);

        var category = await _projectCategoryRepository.GetByIdAsync(id);
        if (category is null) return Result<bool>.Failure("Category not found", 404);

        // التأكد إن الـ Slug الجديد مش مستخدم في "قسم تاني" (باستثناء القسم الحالي)
        if (await _projectCategoryRepository.SlugExistsAsync(request.Slug, id, ct))
        {
            return Result<bool>.Failure("This slug is already in use by another category", 400);
        }


        // التعامل مع الـ UpdateIconUrl

        if (request.IconUrl is not null)
        {
            // رفع الصورة الجديدة
            var uploadResult = await _fileService.UploadAsync(request.IconUrl, "categories");

            if (!uploadResult.IsSuccess)
            {
                return Result<bool>.Failure("uploadIconUrlIsFail", uploadResult.StatusCode);
            }

            category.IconUrl = uploadResult.Value;
        }

        category.Name = request.Name;
        category.Slug = request.Slug;
        category.DisplayOrder = request.DisplayOrder;

        await _projectCategoryRepository.UpdateAsync(category);
        return Result<bool>.Success(true);
    }
}

