
using System.ComponentModel.DataAnnotations;


namespace LetopiaPlatform.Core.DTOs.Comment;

public record UpdateCommentRequest
{
    [Required]
    public string Content { get; init; } = string.Empty;
}
