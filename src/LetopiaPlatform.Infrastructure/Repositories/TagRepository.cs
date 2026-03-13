using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;

public class TagRepository : GenericRepository<Tag>, ITagRepository
{
    private readonly ApplicationDbContext _context;

    public TagRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // ── GetByTargetAsync ──────────────────────────────────────────────────────

    public async Task<IReadOnlyList<Tag>> GetByTargetAsync(
        TagTarget targetType, Guid targetId, CancellationToken ct = default)
        => await _context.Tags
            .Where(t => t.TargetType == targetType && t.TargetId == targetId)
            .OrderBy(t => t.TagName)
            .ToListAsync(ct);

    // ── GetByTargetsAsync ─────────────────────────────────────────────────────

    /// <summary>
    /// Loads tags for an entire page of entities in one DB round-trip.
    /// Returns an ILookup so the caller can do O(1) per-entity grouping:
    ///   var tags = await _tagRepo.GetByTargetsAsync(...);
    ///   var resourceTags = tags[resourceId].Select(t => t.TagName).ToList();
    /// </summary>
    public async Task<ILookup<Guid, Tag>> GetByTargetsAsync(
        TagTarget targetType, IEnumerable<Guid> targetIds, CancellationToken ct = default)
    {
        var ids = targetIds.ToList();

        var tags = await _context.Tags
            .Where(t => t.TargetType == targetType && ids.Contains(t.TargetId))
            .ToListAsync(ct);

        return tags.ToLookup(t => t.TargetId);
    }

    // ── ReplaceTagsAsync ──────────────────────────────────────────────────────

    /// <summary>
    /// Deletes existing tags for the target via ExecuteDeleteAsync (no
    /// change-tracking overhead), then adds the new ones to the context.
    /// Caller commits via IUnitOfWork.SaveChangesAsync.
    /// </summary>
    public async Task ReplaceTagsAsync(
        TagTarget targetType, Guid targetId,
        IEnumerable<string> tagNames, CancellationToken ct = default)
    {
        // Atomic delete of old tags — no need to load them first
        await _context.Tags
            .Where(t => t.TargetType == targetType && t.TargetId == targetId)
            .ExecuteDeleteAsync(ct);

        var now = DateTime.UtcNow;
        var newTags = tagNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => new Tag
            {
                Id = Guid.NewGuid(),
                TargetType = targetType,
                TargetId = targetId,
                TagName = name.Trim().ToLowerInvariant(),
                CreatedAt = now,
            });

        await _context.Tags.AddRangeAsync(newTags, ct);
    }

    // ── DeleteByTargetAsync ───────────────────────────────────────────────────

    public async Task DeleteByTargetAsync(
        TagTarget targetType, Guid targetId, CancellationToken ct = default)
        => await _context.Tags
            .Where(t => t.TargetType == targetType && t.TargetId == targetId)
            .ExecuteDeleteAsync(ct);
}
