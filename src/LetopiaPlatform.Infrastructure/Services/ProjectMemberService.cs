using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Project.Response;
using LetopiaPlatform.Core.DTOs.ProjectMember.Request;
using LetopiaPlatform.Core.DTOs.ProjectMember.Response;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Interfaces.Repositories;

namespace LetopiaPlatform.Infrastructure.Services;
public class ProjectMemberService : IProjectMemberService
{
    private readonly IProjectMemberRepository _memberRepo;
    public ProjectMemberService(IProjectMemberRepository memberRepo) => _memberRepo = memberRepo;

    public async Task<Result<bool>> JoinProjectAsync(ProjectMemberRequestDto request, CancellationToken ct = default)
    {
        var project = await _memberRepo.GetProjectByNameAsync(request.Projecttitle, ct);
        if (project == null) return Result<bool>.Failure("Project not found", 404);

        if (await _memberRepo.IsMemberAsync(project.Id, request.UserId, ct))
            return Result<bool>.Failure("Already a member of this project", 400);

        var member = new ProjectMember
        {
            ProjectId = project.Id,
            MemberId = request.UserId,
            Role = ProjectMemberRole.Contributor
        };


        await _memberRepo.AddAsync(member);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> LeaveProjectAsync(ProjectMemberRequestDto request, CancellationToken ct = default)
    {
        var project = await _memberRepo.GetProjectByNameAsync(request.Projecttitle, ct);
        if (project == null) return Result<bool>.Failure("Project not found", 404);

        var membership = await _memberRepo.GetMembershipAsync(project.Id, request.UserId, ct);
        if (membership == null) return Result<bool>.Failure("Membership record not found", 404);

        await _memberRepo.DeleteAsync(membership);
        return Result<bool>.Success(true);
    }

    public async Task<Result<ProjectMembersListDto>> GetProjectMembersAsync(Guid projectId, CancellationToken ct = default)
    {
        var members = await _memberRepo.GetProjectMembersAsync(projectId, ct);

        var memberDtos = members.Select(m => new ProjectMemberResponseDto(
            m.MemberId,
            m.Member?.FullName ?? "Unknown",
            m.Member?.AvatarUrl ?? "Unknown",
            m.Role.ToString(),
            m.JoinedAt
        )).ToList();

        return Result<ProjectMembersListDto>.Success(new ProjectMembersListDto(memberDtos, memberDtos.Count));
    }

    public async Task<Result<List<ProjectDiscoverResponseDto>>> GetMyProjectsAsync(Guid userId, CancellationToken ct = default)
    {
        var projects = await _memberRepo.GetUserProjectsAsync(userId, ct);
        var response = projects.Select(MapToProjectDto).ToList();
        return Result<List<ProjectDiscoverResponseDto>>.Success(response);
    }

    // --- Helper Methods ---
    private static ProjectDiscoverResponseDto MapToProjectDto(Project p) => new(
        p.Id,
        p.Title,
        p.Category?.Name ?? "General",
        p.DifficultyLevel?.ToString(),
        p.Status.ToString(),
        p.RequiredSkills,
        p.CoverImageUrl,
        p.ProgressPercentage,
        p.Members.Count,
        CalculateTimeLeft(p.Deadline)
    );

    private static string CalculateTimeLeft(DateTime deadline)
    {
        var diff = deadline - DateTime.UtcNow;
        if (diff.TotalDays <= 0) return "Expired";
        if (diff.TotalDays >= 30) return $"{(int)(diff.TotalDays / 30)} months left";
        if (diff.TotalDays >= 7) return $"{(int)(diff.TotalDays / 7)} weeks left";
        return $"{(int)diff.TotalDays} days left";
    }
}
