using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Category;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Exceptions;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IUnitOfWork<ApplicationDbContext> unitOfWork,
        ILogger<CategoryService> logger
    )
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CategoryDto> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<CategoryType>(request.Type, ignoreCase: true, out var type))
        {
            throw new AppException($"Invalid category type {request.Type}", 400);
        }

        if (await _categoryRepository.SlugExistsAsync(request.Name, type, ct: ct))
        {
            throw new ConflictException($"A {type} category with a similar name already exists.");
        }

        var slug = await SlugGenerator.GenerateUniqueAsync(
            request.Name,
            async candidate => await _categoryRepository.SlugExistsAsync(candidate, type, ct: ct));

        var category = new Category
        {
            Name = request.Name,
            Slug = slug,
            IconUrl = request.IconUrl,
            Type = type
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
        category.IconUrl = request.IconUrl;

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
            throw new ConflictException("Cannot delete a category that has communities linked to it.");
        }

        _categoryRepository.Delete(category);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted {Type} category '{Name}' (slug: {Slug})", category.Type, category.Name, category.Slug);
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
            category.Type.ToString()
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