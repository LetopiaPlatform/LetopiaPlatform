using LetopiaPlatform.Core.Common;

namespace LetopiaPlatform.Core.Entities;
public class ProjectCategory : BaseEntity
{
    public required string Name { get; set; }

    // الـ Slug مهم جداً للروابط والفلترة كما شرحنا
    public required string Slug { get; set; }

    // رابط الأيقونة (مثلاً SVG أو URL) لتظهر في الـ Chips
    public string? IconUrl { get; set; }

    // ترتيب الظهور (Web الأول ثم Mobile وهكذا)
    public int DisplayOrder { get; set; }

    // Navigation Property
    // علاقة One-to-Many: القسم الواحد يحتوي على قائمة مشاريع
    // جعلناها virtual لدعم الـ Lazy Loading إذا احتجته مستقبلاً
    public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
}
