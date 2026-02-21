namespace LetopiaPlatform.Core.DTOs.Community;

/// <summary>
/// Community card for the "My Communities" section.
/// Wraps the standard summary with membership context.
/// </summary>
public sealed record JoinedCommunitySummaryDto(
    CommunitySummaryDto Community,
    DateTime JoinedAt);