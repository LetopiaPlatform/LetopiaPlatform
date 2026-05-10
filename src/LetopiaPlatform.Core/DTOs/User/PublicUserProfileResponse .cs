using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.Core.Common;

namespace LetopiaPlatform.Core.DTOs.User;
public sealed record PublicUserProfileResponse(
    Guid Id,
    string FullName,
    string? Email,
    string Role,
    string? PhoneNumber,
    string? Bio,
    string? AvatarUrl,
    string? Location,
    int TotalPoints,
    int CurrentStreak,
   List<string> Skills,
    List<string> Interests,
    DateTime JoinedAt,
    IReadOnlyList<SocialLink>? SocialLinks
);
