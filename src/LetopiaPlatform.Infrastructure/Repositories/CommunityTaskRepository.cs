using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;
public class CommunityTaskRepository : GenericRepository<CommunityTask>, ICommunityTaskRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<CommunityTask> _communityTasks;

    public CommunityTaskRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;

        _communityTasks = _context.Set<CommunityTask>();
    }
}
