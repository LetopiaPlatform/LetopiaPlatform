using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bokra.Core.DTOs.UserProfile.Response
{
    public record ProfileResponse
    (
        string? FullName,
        string? Bio,
        string? AvatarUrl,
        string Role,
        int TotalPoints
    );
}
