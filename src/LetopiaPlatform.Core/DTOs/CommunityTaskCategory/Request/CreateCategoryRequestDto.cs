namespace LetopiaPlatform.Core.DTOs.CommunityTaskCategory.Request;
public record CreateCategoryRequestDto(
    string Name,
    string ColorHex = "#6366f1",
    string? IconKey = null
);
