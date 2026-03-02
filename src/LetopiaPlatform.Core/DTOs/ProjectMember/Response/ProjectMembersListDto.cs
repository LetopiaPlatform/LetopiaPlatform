namespace LetopiaPlatform.Core.DTOs.ProjectMember.Response;
public record ProjectMembersListDto(
    List<ProjectMemberResponseDto> Members,
    int TotalCount
);
