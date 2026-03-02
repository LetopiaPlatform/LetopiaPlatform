using LetopiaPlatform.Core.Entities;

namespace LetopiaPlatform.Core.Interfaces.Repositories;
public interface IProjectMemberRepository : IGenericRepository<ProjectMember>
{
    Task<List<Project>> GetUserProjectsAsync(Guid userId, CancellationToken ct);
    Task<List<ProjectMember>> GetProjectMembersAsync(Guid projectId, CancellationToken ct);
    Task<ProjectMember?> GetMembershipAsync(Guid projectId, Guid memberId, CancellationToken ct);
    Task<bool> IsMemberAsync(Guid projectId, Guid memberId, CancellationToken ct);
    Task<Project?> GetProjectByNameAsync(string projectName, CancellationToken ct);
}
