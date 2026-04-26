using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Project.Request;
using LetopiaPlatform.Core.DTOs.Project.Response;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Core.Services.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Infrastructure.Services;
public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepo;
    private readonly IFileStorageService _fileService;
    private readonly ILogger<ProjectService> _logger;
    private IUnitOfWork<ApplicationDbContext> _unitOfWork;
    public ProjectService(IProjectRepository projectRepo, IFileStorageService fileService, ILogger<ProjectService> logger,
                  IUnitOfWork<ApplicationDbContext> unitOfWork)
    {
        _projectRepo = projectRepo;
        _fileService = fileService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginatedResult<ProjectDiscoverResponseDto>>> GetDiscoverAsync(ProjectFilterDto filter, CancellationToken ct = default)
    {
        var paginatedProjects = await _projectRepo.GetFilteredProjectsAsync(filter, ct);

        var responses = paginatedProjects.Items.Select(MapToProjectDto).ToList();

        var result = PaginatedResult<ProjectDiscoverResponseDto>.Create(
            responses,
            paginatedProjects.TotalItems,
            paginatedProjects.Page,
            paginatedProjects.PageSize
        );

        return Result<PaginatedResult<ProjectDiscoverResponseDto>>.Success(result);
    }

    public async Task<Result<ProjectDetailsResponseDto>> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetProjectWithDetailsAsync(id, ct);
        if (project is null) return Result<ProjectDetailsResponseDto>.Failure("Project not found", 404);

        return Result<ProjectDetailsResponseDto>.Success(MapToDetails(project));
    }

    public async Task<Result<Guid>> CreateAsync(Guid ownerId, CreateProjectRequestDto request, CancellationToken ct = default)
    {
        var exists = await _projectRepo.IsTitleExistsInCategoryAsync(request.Title, request.CategoryId, ct);

        if (exists)
        {
            _logger.LogWarning("Project creation failed: Title '{Title}' already exists in Category {CategoryId}",
                request.Title, request.CategoryId);

            return Result<Guid>.Failure("A project with this title already exists in this category.", 400);
        }

        // 1. Handling Cover Image Upload
        string? coverUrl = null;
        if (request.CoverImage != null)
        {
            var upload = await _fileService.UploadAsync(request.CoverImage, "projects", ct);
            if (!upload.IsSuccess)
                return Result<Guid>.Failure("Image upload failed", 400);

            coverUrl = upload.Value;
        }

        var project = new Project
        {
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            OwnerId = ownerId,
            CoverImageUrl = coverUrl,

            // Fix: Force UTC for PostgreSQL (timestamp with time zone)
            StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
            Deadline = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc),

            // New UI Fields
            IsPublic = request.IsPublic,
            RequiredSkills = request.RequiredSkills ?? [],
            Goals = request.Goals ?? [],
            TimelineEvents = request.TimelineEvents ?? [],

            // Parsing Enum safely
            DifficultyLevel = Enum.TryParse<DifficultyLevel>(request.DifficultyLevel, true, out var diff) ? diff : null,

            Status = ProjectStatus.Recruiting,


            Milestones = request.Milestones.Select(m => new ProjectMilestoneDetails
            {
                Title = m.Title,
                Description = m.Description,
                CreatedAt = DateTime.UtcNow
            }).ToList()
        };

        try
        {
            await _projectRepo.AddAsync(project);

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Project {ProjectId} created successfully", project.Id);
            return Result<Guid>.Success(project.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating project");
            return Result<Guid>.Failure($"Database Error: {ex.Message}", 500);
        }
    }

    //-----UpdateProject-----------------------------------------------------
    public async Task<Result<string>> UpdateAsync(Guid id, Guid userId, UpdateProjectRequestDto request, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(id);

        if (project is null) return Result<string>.Failure("Project not found", 404);

        if (project.OwnerId != userId)
            return Result<string>.Failure("You are not authorized to update this project", 403);


        if (request.CoverImage != null)
        {
            var upload = await _fileService.UploadAsync(request.CoverImage, "projects", ct);
            if (upload.IsSuccess) project.CoverImageUrl = upload.Value;
        }

        project.Title = request.Title;
        project.Description = request.Description;
        project.CategoryId = request.CategoryId;
        project.StartDate = request.StartDate;
        project.Deadline = request.EndDate;
        project.RequiredSkills = request.RequiredSkills;
        project.Goals = request.Goals;

        project.DifficultyLevel = Enum.TryParse<DifficultyLevel>(request.DifficultyLevel, out var diff)
            ? diff : project.DifficultyLevel;

        await _projectRepo.UpdateAsync(project);
        return Result<string>.Success("UpdateOperationIsSuccessfully");
    }
    //-----DeleteProject-----------------------------------------------------
    public async Task<Result<string>> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(id);
        if (project is null) return Result<string>.Failure("Project not found", 404);

        if (project.OwnerId != userId)
            return Result<string>.Failure("You are not authorized to delete this project", 403);

        await _projectRepo.DeleteAsync(project);
        return Result<string>.Success("DeleteOperationIsSuccessfully");
    }


    // ── Mapping Helpers ────────────────────────────────────────────────────
    private static ProjectDiscoverResponseDto MapToProjectDto(Project p) => new(
    p.Id,
    p.Title,
    p.Category?.Name ?? "General",
    p.DifficultyLevel?.ToString(),
    p.Status.ToString(),
    p.RequiredSkills,
    p.CoverImageUrl,
    p.Members.Count,               // MembersCount
    CalculateTimeLeft(p.Deadline), // TimeLeft
    p.OwnerId,                     // OwnerId
    p.Owner?.FullName ?? "Unknown" // OwnerName
    );

    private static ProjectDetailsResponseDto MapToDetails(Project p) => new(
     p.Id,
     p.Title,
     p.Description,
     p.Category?.Name ?? "General",
     CalculateTimeLeft(p.Deadline), // TimeLeftText
     p.RequiredSkills,
     p.Goals,                       // ProjectGoals
     p.TimelineEvents ?? [],        // TimelineEvents
     p.StartDate,
     p.Deadline,
     p.CoverImageUrl,
     p.Status.ToString(),
     p.Owner?.FullName ?? "Unknown", // OwnerName
     p.Milestones.Select(m => new MilestoneResponseDto(
         m.Title,
         m.Description,
         m.CreatedAt
     )).ToList()                     // Milestones
 );
    private static string CalculateTimeLeft(DateTime deadline)
    {
        var diff = deadline - DateTime.UtcNow;
        if (diff.TotalDays <= 0) return "Expired";
        if (diff.TotalDays >= 7) return $"{(int)(diff.TotalDays / 7)} weeks left";
        return $"{(int)diff.TotalDays} days left";
    }
}
