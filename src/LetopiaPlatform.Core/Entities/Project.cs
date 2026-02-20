using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities.Identity;

namespace LetopiaPlatform.Core.Entities;
public class Project : AuditableEntity
{
    public required string Title { get; set; }
    public required string Description { get; set; }

    // ليفل الصعوبة: Beginner, Intermediate, Advanced
    public string? DifficultyLevel { get; set; }

    public DateTime Deadline { get; set; }

    // هل المشروع اكتمل عدده؟
    public bool IsFull { get; set; }

    // ---- الإضافات الجديدة ----

    // حالة المشروع: Recruiting, InProgress, Completed
    public string Status { get; set; } = "Recruiting";

    // الحد الأقصى للأعضاء (مثلاً مشروع محتاج 4 طلاب بس)
    public int MaxMembers { get; set; } = 5;

    // المهارات المطلوبة (مخزنة كقائمة نصوص)
    public List<string> RequiredSkills { get; set; } = [];

    // رابط صورة غلاف للمشروع (لو حابب شكل الكارت يبقى أشيك)
    public string? CoverImageUrl { get; set; }



    // 1. ضيف الـ Foreign Key ده
    public Guid CategoryId { get; set; }

    // 2. ضيف الـ Navigation Property دي (هي دي اللي ناقصة ومسببة الـ Error)
    public virtual ProjectCategory Category { get; set; } = null!;

    // صاحب المشروع
    public Guid OwnerId { get; set; }
    public virtual User Owner { get; set; } = null!;
}
