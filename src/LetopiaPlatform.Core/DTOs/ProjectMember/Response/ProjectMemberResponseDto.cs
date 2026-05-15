using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.ProjectMember.Response;
public record ProjectMemberResponseDto(
    Guid MemberId,
    string MemberName,
    string? ProfilePictureUrl,
    ProjectMemberRole Role,
    DateTime JoinedAt
);
