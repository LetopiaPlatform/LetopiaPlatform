using LetopiaPlatform.Core.DTOs.Community;
using LetopiaPlatform.Core.DTOs.User;

namespace LetopiaPlatform.Core.DTOs.Search;

public sealed record GlobalSearchResultDto(
    List<CommunitySummaryDto> Communities,
    List<ProjectSearchResultDto> Projects,
    List<UserSummaryDto> Members);