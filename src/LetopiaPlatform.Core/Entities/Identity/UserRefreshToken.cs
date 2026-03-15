using System.ComponentModel.DataAnnotations;
using LetopiaPlatform.Core.Common;

namespace LetopiaPlatform.Core.Entities.Identity;
public class UserRefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string? Token { get; set; }
    public string? RefreshTokenHash { get; set; }
    public string? JwtId { get; set; }

    [ConcurrencyCheck]
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime AddedTime { get; set; }
    public DateTime ExpiryDate { get; set; }

    public virtual User? User { get; set; }
}
