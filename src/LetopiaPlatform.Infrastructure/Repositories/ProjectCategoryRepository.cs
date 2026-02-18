using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;
public class ProjectCategoryRepository : GenericRepository<ProjectCategory>, IProjectCategoryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<ProjectCategory> _projectCategories;

    public ProjectCategoryRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;

        _projectCategories = _context.Set<ProjectCategory>();
    }
}
