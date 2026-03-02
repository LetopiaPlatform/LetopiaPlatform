namespace LetopiaPlatform.Core.DTOs.ProjectMember.Response;
public record ProjectMemberResponseDto(
    Guid MemberId,
    string MemberName,
    string? ProfilePictureUrl,
    string Role,
    DateTime JoinedAt
);
