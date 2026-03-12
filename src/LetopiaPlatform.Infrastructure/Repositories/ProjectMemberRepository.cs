using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;
public class ProjectMemberRepository : GenericRepository<ProjectMember>, IProjectMemberRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<ProjectMember> _projectMembers;

    public ProjectMemberRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
        _projectMembers = _context.Set<ProjectMember>();
    }

    public async Task<Project?> GetProjectByNameAsync(string projectName, CancellationToken ct)
    {
        return await _context.Projects
            .FirstOrDefaultAsync(p => EF.Functions.Like(p.Title, projectName), ct);
    }

    public async Task<bool> IsMemberAsync(Guid projectId, Guid memberId, CancellationToken ct)
    {
        return await _projectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.MemberId == memberId, ct);
    }

    public async Task<ProjectMember?> GetMembershipAsync(Guid projectId, Guid memberId, CancellationToken ct)
    {
        return await _projectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.MemberId == memberId, ct);
    }

    public async Task<List<ProjectMember>> GetProjectMembersAsync(Guid projectId, CancellationToken ct)
    {
        return await _projectMembers
            .Where(pm => pm.ProjectId == projectId)
            .Include(pm => pm.Member) // جلب بيانات الـ User المرتبط
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<List<Project>> GetUserProjectsAsync(Guid userId, CancellationToken ct)
    {
        return await _projectMembers
            .Where(m => m.MemberId == userId)
            .Include(m => m.Project)
                .ThenInclude(p => p.Category)
            .Include(m => m.Project)
                .ThenInclude(p => p.Members)
            .Select(m => m.Project)
            .AsNoTracking()
            .ToListAsync(ct);
    }

}
