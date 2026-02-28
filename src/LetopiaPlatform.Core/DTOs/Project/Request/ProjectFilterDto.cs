namespace LetopiaPlatform.Core.DTOs.Project.Request;
/// <summary>
/// Request DTO for filtering and paginating projects in the Discover and List screens.
/// </summary>
public class ProjectFilterDto
{
    // ── Search & Filter (From UI) ──────────────────────────────────────────

    public string? SearchTerm { get; set; }

    public Guid? CategoryId { get; set; }

    // ── Pagination Parameters ─────────────────────────────────────────────

    private int _page = 1;
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 50 ? 50 : (value < 1 ? 10 : value);
    }
}
