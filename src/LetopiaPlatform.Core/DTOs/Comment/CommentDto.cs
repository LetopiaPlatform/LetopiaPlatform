using System;
using LetopiaPlatform.Core.DTOs.Author;

namespace LetopiaPlatform.Core.DTOs.Comment;

public sealed record CommentDto(
    Guid Id,
    Guid PostId,
    AuthorDto Author = null!,
    string Content = "",
    int Upvotes = 0,
    DateTime CreatedAt = default,
    DateTime? UpdatedAt = null,
    string? CurrentUserReaction = null
);
