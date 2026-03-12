namespace LetopiaPlatform.Core.DTOs.CommunityTaskCategory.Request;
public record UpdateCategoryRequestDto(
    string Name,
    string ColorHex,
    string? IconKey
);
