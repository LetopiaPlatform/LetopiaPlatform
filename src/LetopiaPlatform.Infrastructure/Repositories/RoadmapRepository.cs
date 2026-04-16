using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;

internal sealed class RoadmapRepository : IRoadmapRepository
{
    private readonly ApplicationDbContext _context;

    public RoadmapRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Roadmap?> GetByIdWithPhasesAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Roadmaps

            .AsNoTracking()
            .Include(r => r.Phases.OrderBy(p => p.Order))
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<List<Roadmap>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Roadmaps

            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<RoadmapPhase?> GetPhaseByIdAsync(Guid phaseId, CancellationToken ct = default)
    {
        return await _context.RoadmapPhases

            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == phaseId, ct);
    }

    public void Add(Roadmap roadmap)
    {
        _context.Roadmaps.Add(roadmap);
    }

    public void Update(Roadmap roadmap)
    {
        _context.Roadmaps.Update(roadmap);
    }
}
