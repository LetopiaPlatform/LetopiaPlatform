using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Project.Request;
using LetopiaPlatform.Core.DTOs.Project.Response;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Infrastructure.Services;
public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepo;
    private readonly IFileStorageService _fileService;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(IProjectRepository projectRepo, IFileStorageService fileService, ILogger<ProjectService> logger)
    {
        _projectRepo = projectRepo;
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<ProjectDiscoverResponseDto>>> GetDiscoverAsync(ProjectFilterDto filter, CancellationToken ct = default)
    {
        var paginatedProjects = await _projectRepo.GetFilteredProjectsAsync(filter, ct);

        var responses = paginatedProjects.Items.Select(MapToDiscover).ToList();

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

    public async Task<Result<Guid>> CreateAsync(CreateProjectRequestDto request, CancellationToken ct = default)
    {
        string? coverUrl = null;
        if (request.CoverImage != null)
        {
            var upload = await _fileService.UploadAsync(request.CoverImage, "projects");
            if (!upload.IsSuccess) return Result<Guid>.Failure("Image upload failed", 400);
            coverUrl = upload.Value;
        }

        var project = new Project
        {
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            OwnerId = request.OwnerId,
            CoverImageUrl = coverUrl,
            StartDate = request.StartDate,
            Deadline = request.EndDate,
            MaxMembers = request.MaxMembers,
            RequiredSkills = request.RequiredSkills,
            Goals = request.Goals,
            DifficultyLevel = Enum.TryParse<DifficultyLevel>(request.DifficultyLevel, out var diff) ? diff : null,
            Status = ProjectStatus.Recruiting,
            ProgressPercentage = 0
        };

        await _projectRepo.AddAsync(project);
        return Result<Guid>.Success(project.Id);
    }

    //-----UpdateProject-----------------------------------------------------
    public async Task<Result<string>> UpdateAsync(Guid id, UpdateProjectRequestDto request, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(id);
        if (project is null) return Result<string>.Failure("Project not found", 404);

        if (request.CoverImage != null)
        {
            var upload = await _fileService.UploadAsync(request.CoverImage, "projects");
            if (upload.IsSuccess) project.CoverImageUrl = upload.Value;
        }

        project.Title = request.Title;
        project.Description = request.Description;
        project.CategoryId = request.CategoryId;
        project.StartDate = request.StartDate;
        project.Deadline = request.EndDate;
        project.MaxMembers = request.MaxMembers;
        project.RequiredSkills = request.RequiredSkills;
        project.Goals = request.Goals;

        project.DifficultyLevel = Enum.TryParse<DifficultyLevel>(request.DifficultyLevel, out var diff)
            ? diff : project.DifficultyLevel;

        await _projectRepo.UpdateAsync(project);
        return Result<string>.Success("UpdateOperationIsSuccessfully");
    }
    //-----DeleteProject-----------------------------------------------------
    public async Task<Result<string>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(id);
        if (project is null) return Result<string>.Failure("Project not found", 404);

        await _projectRepo.DeleteAsync(project);
        return Result<string>.Success("DeleteOperationIsSuccessfully");
    }


    // ── Mapping Helpers ────────────────────────────────────────────────────
    private static ProjectDiscoverResponseDto MapToDiscover(Project p) => new(
        p.Id, p.Title, p.Category?.Name ?? "General", p.DifficultyLevel?.ToString(),
        p.Status.ToString(), p.RequiredSkills, p.CoverImageUrl
    );

    private static ProjectDetailsResponseDto MapToDetails(Project p) => new(
        p.Id, p.Title, p.Description, p.Category?.Name ?? "General",
        p.ProgressPercentage, CalculateTimeLeft(p.Deadline),
        p.RequiredSkills, p.Goals, p.StartDate, p.Deadline, p.CoverImageUrl, p.Status.ToString()
    );

    private static string CalculateTimeLeft(DateTime deadline)
    {
        var diff = deadline - DateTime.UtcNow;
        if (diff.TotalDays <= 0) return "Expired";
        if (diff.TotalDays >= 7) return $"{(int)(diff.TotalDays / 7)} weeks left";
        return $"{(int)diff.TotalDays} days left";
    }
}
