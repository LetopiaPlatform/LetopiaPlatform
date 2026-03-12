using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;

internal sealed class ConversationRepository : IConversationRepository
{
    private readonly ApplicationDbContext _context;

    public ConversationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AgentConversation?> GetByIdWithMessagesAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.AgentConversations
<<<<<<< HEAD
=======
            .AsNoTracking()
>>>>>>> main
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<AgentConversation?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.AgentConversations
<<<<<<< HEAD
=======
            .AsNoTracking()
>>>>>>> main
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<List<AgentConversation>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.AgentConversations
<<<<<<< HEAD
=======
            .AsNoTracking()
>>>>>>> main
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(ct);
    }

    public void Add(AgentConversation conversation)
    {
        _context.AgentConversations.Add(conversation);
    }

    public void AddMessage(ConversationMessage message)
    {
        _context.ConversationMessages.Add(message);
    }
}
