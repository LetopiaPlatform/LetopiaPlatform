using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LetopiaPlatform.Core.Entities;
/// <summary>
/// Free-text tag attached to a resource (e.g. "ASP.NET", "Docker", "AI").
/// Used for filtering and recommendation scoring.
/// A unique index prevents the same tag appearing twice on one resource.
/// </summary>
public class ResourceTag
{
    public Guid Id { get; set; }

    public Guid ResourceId { get; set; }

    public string TagName { get; set; } = default!;

    // ── Navigation ────────────────────────────────────────────────────────────

    public CommunityResource Resource { get; set; } = default!;
}
