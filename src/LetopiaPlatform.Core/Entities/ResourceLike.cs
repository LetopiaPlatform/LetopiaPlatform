using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.Core.Entities.Identity;

namespace LetopiaPlatform.Core.Entities;
/// <summary>
/// One row per (UserId, ResourceId) pair.
/// A unique index at the DB level prevents a user from liking the same
/// resource twice. ToggleLike adds or removes this row accordingly.
/// </summary>
public class ResourceLike
{
    public Guid Id { get; set; }

    public Guid ResourceId { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────

    public CommunityResource Resource { get; set; } = default!;

    public User User { get; set; } = default!;
}
