using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;
public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<Project> _projects;

    public ProjectRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
        _projects = _context.Set<Project>();
    }


}
