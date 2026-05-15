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
    private readonly IGenericRepository<ProjectMilestoneDetails> _milestoneRepo;

    public ProjectService(IProjectRepository projectRepo, IFileStorageService fileService, ILogger<ProjectService> logger,
                  IUnitOfWork<ApplicationDbContext> unitOfWork, IGenericRepository<ProjectMilestoneDetails> milestoneRepo)
    {
        _projectRepo = projectRepo;
        _fileService = fileService;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _milestoneRepo = milestoneRepo;
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



        var project = new Project
        {
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            OwnerId = ownerId,


            IsPublic = request.IsPublic,
            RequiredSkills = request.RequiredSkills ?? [],

            DifficultyLevel = Enum.TryParse<DifficultyLevel>(request.DifficultyLevel, true, out var diff) ? diff : null,
            Status = ProjectStatus.Available,

        };

        if (request.Links != null && request.Links.Count > 0)
        {
            foreach (var linkUrl in request.Links)
            {
                project.Resources.Add(new ProjectResource
                {
                    Id = Guid.NewGuid(),
                    Name = "Project Link",
                    Url = linkUrl,
                    IsFile = false
                });
            }
        }

        if (request.Files != null && request.Files.Count > 0)
        {
            foreach (var file in request.Files)
            {
                var uploadResult = await _fileService.UploadAsync(file, "project-resources", ct);
                if (uploadResult.IsSuccess)
                {
                    project.Resources.Add(new ProjectResource
                    {
                        Id = Guid.NewGuid(),
                        Name = file.FileName,
                        Url = uploadResult.Value ?? string.Empty,
                        IsFile = true
                    });
                }
            }
        }

        try
        {
            await _projectRepo.AddAsync(project);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Project {ProjectId} created successfully with {ResourcesCount} resources",
                project.Id, project.Resources.Count);

            return Result<Guid>.Success(project.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating project: {Title}", request.Title);
            return Result<Guid>.Failure($"Database Error: {ex.Message}", 500);
        }
    }

    //-----UpdateProject-----------------------------------------------------
    public async Task<Result<string>> UpdateAsync(Guid id, Guid userId, UpdateProjectRequestDto request, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(id);

        if (project is null)
            return Result<string>.Failure("Project not found", 404);

        if (project.OwnerId != userId)
            return Result<string>.Failure("You are not authorized to update this project", 403);



        project.Title = request.Title;
        project.Description = request.Description;
        project.CategoryId = request.CategoryId;
        project.RequiredSkills = request.RequiredSkills ?? [];

        if (Enum.TryParse<DifficultyLevel>(request.DifficultyLevel, true, out var diff))
        {
            project.DifficultyLevel = diff;
        }

        if (request.Links != null && request.Links.Count > 0)
        {
            foreach (var url in request.Links)
            {
                project.Resources.Add(new ProjectResource
                {
                    Id = Guid.NewGuid(),
                    Name = "Project Link",
                    Url = url,
                    IsFile = false
                });
            }
        }

        if (request.Files != null && request.Files.Count > 0)
        {
            foreach (var file in request.Files)
            {
                var uploadResult = await _fileService.UploadAsync(file, "project-resources", ct);
                if (uploadResult.IsSuccess)
                {
                    project.Resources.Add(new ProjectResource
                    {
                        Id = Guid.NewGuid(),
                        Name = file.FileName,
                        Url = uploadResult.Value!,
                        IsFile = true
                    });
                }
            }
        }

        try
        {
            await _projectRepo.UpdateAsync(project);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Project {ProjectId} updated successfully by user {UserId}", id, userId);
            return Result<string>.Success("Project updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating project {ProjectId}", id);
            return Result<string>.Failure($"Database Error: {ex.Message}", 500);
        }
    }
    //-----DeleteProject-----------------------------------------------------
    public async Task<Result<string>> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(id);

        if (project is null)
            return Result<string>.Failure("Project not found", 404);

        if (project.OwnerId != userId)
        {
            _logger.LogWarning("Unauthorized delete attempt for project {ProjectId} by user {UserId}", id, userId);
            return Result<string>.Failure("You are not authorized to delete this project", 403);
        }

        try
        {

            if (project.Resources != null && project.Resources.Count > 0)
            {
                foreach (var resource in project.Resources.Where(r => r.IsFile))
                {
                    await _fileService.DeleteAsync(resource.Url, ct);
                }
            }

            await _projectRepo.DeleteAsync(project);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Project {ProjectId} and its related files deleted successfully", id);
            return Result<string>.Success("Project deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting project {ProjectId}", id);
            return Result<string>.Failure($"Database Error: {ex.Message}", 500);
        }
    }
    //--------------------------AddMilestoneToProject-----------------------------------------------------

    public async Task<Result<MilestoneResponseDto>> AddMilestoneAsync(Guid userId, Guid projectId, MilestoneRequestDto dto, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetProjectWithDetailsAsync(projectId, ct);
        if (project is null) return Result<MilestoneResponseDto>.Failure("Project not found", 404);

        if (project.OwnerId != userId)
            return Result<MilestoneResponseDto>.Failure("Unauthorized", 403);

        var milestone = new ProjectMilestoneDetails
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            DurationText = dto.DurationText,
            Status = dto.Status,
            ProjectId = projectId,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _milestoneRepo.AddAsync(milestone);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<MilestoneResponseDto>.Success(new MilestoneResponseDto(
                milestone.Id, milestone.Title, milestone.Description, milestone.DurationText,
                milestone.Status.ToString(), project.CalculatedProgress));
        }
        catch (Exception ex)
        {
            return Result<MilestoneResponseDto>.Failure($"Error: {ex.Message}", 500);
        }
    }

    public async Task<Result<MilestoneResponseDto>> UpdateMilestoneAsync(Guid userId, Guid milestoneId, MilestoneRequestDto dto, CancellationToken ct = default)
    {

        var milestone = await _milestoneRepo
            .GetByIdAsync(milestoneId);

        if (milestone is null)
            return Result<MilestoneResponseDto>.Failure("Milestone not found", 404);


        var project = await _projectRepo.GetProjectWithDetailsAsync(milestone.ProjectId, ct);

        if (project is null || project.OwnerId != userId)
        {
            _logger.LogWarning("Unauthorized update attempt for milestone {MilestoneId} by user {UserId}", milestoneId, userId);
            return Result<MilestoneResponseDto>.Failure("You are not authorized to update this milestone", 403);
        }


        milestone.Title = dto.Title;
        milestone.Description = dto.Description;
        milestone.DurationText = dto.DurationText;
        milestone.Status = dto.Status;
        milestone.UpdatedAt = DateTime.UtcNow;

        try
        {

            await _milestoneRepo.UpdateAsync(milestone);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Milestone {MilestoneId} updated successfully", milestoneId);

            return Result<MilestoneResponseDto>.Success(new MilestoneResponseDto(
                milestone.Id,
                milestone.Title,
                milestone.Description,
                milestone.DurationText,
                milestone.Status.ToString(),
                project.CalculatedProgress

            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating milestone {MilestoneId}", milestoneId);
            return Result<MilestoneResponseDto>.Failure($"Database Error: {ex.Message}", 500);
        }
    }



    public async Task<Result<int>> DeleteMilestoneAsync(Guid userId, Guid milestoneId, CancellationToken ct = default)
    {
        var milestone = await _milestoneRepo.GetByIdAsync(milestoneId);
        if (milestone is null) return Result<int>.Failure("Milestone not found", 404);

        var project = await _projectRepo.GetProjectWithDetailsAsync(milestone.ProjectId, ct);
        if (project is null || project.OwnerId != userId)
            return Result<int>.Failure("Unauthorized", 403);

        try
        {
            await _milestoneRepo.DeleteAsync(milestone);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<int>.Success(project.CalculatedProgress);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error: {ex.Message}", 500);
        }
    }

    public async Task<Result<MilestoneResponseDto>> ToggleMilestoneStatusAsync(Guid userId, Guid milestoneId, CancellationToken ct = default)
    {
        var milestone = await _milestoneRepo.GetByIdAsync(milestoneId);
        if (milestone is null) return Result<MilestoneResponseDto>.Failure("Milestone not found", 404);

        var project = await _projectRepo.GetProjectWithDetailsAsync(milestone.ProjectId, ct);
        if (project is null || project.OwnerId != userId)
            return Result<MilestoneResponseDto>.Failure("Unauthorized", 403);

        milestone.Status = milestone.Status == MilestoneStatus.Completed
                           ? MilestoneStatus.InProgress
                           : MilestoneStatus.Completed;

        milestone.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _milestoneRepo.UpdateAsync(milestone);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<MilestoneResponseDto>.Success(new MilestoneResponseDto(
                milestone.Id, milestone.Title, milestone.Description, milestone.DurationText,
                milestone.Status.ToString(), project.CalculatedProgress));
        }
        catch (Exception ex)
        {
            return Result<MilestoneResponseDto>.Failure($"Error: {ex.Message}", 500);
        }
    }

    // ── Mapping Helpers ────────────────────────────────────────────────────
    private static ProjectDiscoverResponseDto MapToProjectDto(Project p) => new(
    p.Id,
    p.Title,
    p.Category?.Name ?? "General",
    p.DifficultyLevel?.ToString(),
    p.Status.ToString(),
    p.RequiredSkills,
    p.Members.Count,               // MembersCount
    p.OwnerId,                     // OwnerId
    p.Owner.AvatarUrl ?? "Unknown",           // OwnerAvatarUrl
    p.Owner?.FullName ?? "Unknown" // OwnerName
    );

    private static ProjectDetailsResponseDto MapToDetails(Project p) => new(
    p.Id,
    p.Title,
    p.Description ?? "",
    p.Category?.Name ?? "General",
    p.RequiredSkills,
    p.Status.ToString(),
    p.Owner?.FullName ?? "Unknown",
    p.Owner?.AvatarUrl ?? "Unknown",
    p.CalculatedProgress,
    p.Milestones.Select(m => new MilestoneResponseDto(
        m.Id,
        m.Title,
        m.Description,
        m.DurationText,
        m.Status.ToString(),
        m.Project.CalculatedProgress
    )).ToList(),

    p.Resources.Select(r => new ResourceResponseDto(
        r.Id,
        r.Name,
        r.Url,
        r.IsFile
    )).ToList()
);

}
