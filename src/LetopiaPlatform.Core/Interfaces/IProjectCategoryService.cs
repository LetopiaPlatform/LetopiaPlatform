using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.ProjectCategory.Response;

namespace LetopiaPlatform.Core.Interfaces;
public interface IProjectCategoryService
{
    /// <summary>
    /// جلب جميع الأقسام مرتبة حسب ترتيب العرض المحدد
    /// </summary>
    Task<Result<IEnumerable<CategoryResponse>>> GetAllOrderedAsync(CancellationToken ct = default);

    /// <summary>
    /// جلب تفاصيل قسم معين باستخدام الـ Slug مع المشاريع المتاحة فيه
    /// </summary>
    Task<Result<CategoryResponse>> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// جلب إحصائيات بعدد المشاريع الموجودة في كل قسم
    /// </summary>
    Task<Result<Dictionary<Guid, int>>> GetCategoryStatsAsync(CancellationToken ct = default);

    /// <summary>
    /// حذف قسم بشرط عدم وجود مشاريع مرتبطة به
    /// </summary>
    Task<Result<bool>> DeleteCategoryAsync(Guid id, CancellationToken ct = default);
}
