using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LetopiaPlatform.Core.DTOs.CommunityResourse;
/// <summary>
/// Lightweight user info embedded in ResourceDto so the UI can display
/// "Uploaded by Ahmed · 2h ago" without a separate API call.
/// </summary>
public record UploadedByDto(
    Guid UserId,
    string UserName,
    string? AvatarUrl
);
