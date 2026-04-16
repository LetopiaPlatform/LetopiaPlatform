using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Category;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Exceptions;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Services.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IUnitOfWork<ApplicationDbContext> unitOfWork,
        IFileStorageService fileStorageService,
        ILogger<CategoryService> logger
    )
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _fileStorageService = fileStorageService;
    }

    public async Task<CategoryDto> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<CategoryType>(request.Type, ignoreCase: true, out var type))
        {
            throw new AppException($"Invalid category type {request.Type}", 400);
        }

        // Hierarchy validation
        if (request.ParentCategoryId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, ct)
                ?? throw new NotFoundException("Parent Category", request.ParentCategoryId.Value);

            // Enforce max depth = 2: parent must be a root category.
            if (parent.ParentCategoryId is not null)
            {
                throw new AppException("Sub-categories cannot have children. Maximum category depth is 2.", 400);
            }

            // Enforce type consistency: child must match parent type
            if (parent.Type != type)
            {
                throw new AppException($"Sub-category type must match parent type '{parent.Type}'.", 400);
            }

            // Sub-categories cannot have icons
            if (request.Icon is not null)
            {
                throw new AppException("Only main categories can have icons.", 400);
            }
        }

        var slug = await SlugGenerator.GenerateUniqueAsync(
            request.Name,
            async candidate => await _categoryRepository.SlugExistsAsync(candidate, type, ct: ct));

        string? iconUrl = null;
        if (request.Icon is not null)
        {
            var result = await _fileStorageService.UploadSvgAsync(request.Icon, "categories/icons", ct: ct);
            if (!result.IsSuccess)
            {
                throw new AppException($"Icon upload failed: {result.Error}", 400);
            }
            iconUrl = result.Value;
        }

        var category = new Category
        {
            Name = request.Name,
            Slug = slug,
            IconUrl = iconUrl,
            Type = type,
            ParentCategoryId = request.ParentCategoryId
        };

        _categoryRepository.Add(category);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Created new {Type} category '{Name}' (slug: {Slug})", type, category.Name, category.Slug);

        return MapToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Category", id);
        
        var newSlug = await SlugGenerator.GenerateUniqueAsync(
            request.Name,
            async candidate => await _categoryRepository.SlugExistsAsync(candidate, category.Type, excludeId: id, ct: ct));
        
        category.Name = request.Name;
        category.Slug = newSlug;

        bool isSubCategory = category.ParentCategoryId is not null;

        if (request.Icon is not null && isSubCategory)
        {
            throw new AppException("Only main categories can have icons.", 400);
        }

        if (request.RemoveIcon)
        {
            if (category.IconUrl is not null)
            {
                await _fileStorageService.DeleteAsync(category.IconUrl, ct);
                category.IconUrl = null;
            }
        }
        else if (request.Icon is not null)
        {
            if (category.IconUrl is not null)
            {
                await _fileStorageService.DeleteAsync(category.IconUrl, ct);
            }

            var result = await _fileStorageService.UploadSvgAsync(request.Icon, "categories/icons", ct: ct);
            if (!result.IsSuccess)
            {
                throw new AppException($"Icon upload failed: {result.Error}", 400);
            }
            category.IconUrl = result.Value;
        }

        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Updated {Type} category '{Name}' (slug: {Slug})", category.Type, category.Name, category.Slug);
        return MapToDto(category);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Category", id);

        if (await _categoryRepository.HasDependentsAsync(id, ct))
        {
            throw new ConflictException("Cannot delete a category that has subcategories or communities linked to it.");
        }

        if (category.IconUrl is not null)
        {
            await _fileStorageService.DeleteAsync(category.IconUrl, ct);
        }

        _categoryRepository.Delete(category);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted {Type} category '{Name}' (slug: {Slug})", category.Type, category.Name, category.Slug);
    }

    public async Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Category", id);

        return MapToDto(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetByTypeAsync(
        string type, CancellationToken ct = default)
    {
        var categoryType = ParseType(type);
        var categories = await _categoryRepository.GetByTypeOrderedAsync(categoryType, ct);
        return categories.Select(MapToDto);
    }

    public async Task<CategoryDto> GetBySlugAsync(
        string slug, string type, CancellationToken ct = default)
    {
        var categoryType = ParseType(type);
        var category = await _categoryRepository.GetBySlugAsync(slug, categoryType, ct)
            ?? throw new NotFoundException("Category", slug);

        return MapToDto(category);
    }
    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.IconUrl,
            category.Type.ToString(),
            category.ParentCategoryId,
            category.ChildCategories?.Select(MapToDto).ToList() is { Count: > 0 } children ? children : null
        );
    }

    private static CategoryType ParseType(string type)
    {
        if (!Enum.TryParse<CategoryType>(type, ignoreCase: true, out var categoryType))
        {
            throw new AppException($"Invalid category type '{type}'.", 400);
        }
        return categoryType;
    }
}