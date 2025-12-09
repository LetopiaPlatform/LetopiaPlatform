using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bokra.Core.DTOs.UserProfile.Request
{
    public record UpdatedProfileRequest
    (
        Guid Id,
        string FullName,
        string Bio,
        DateTime? DateOfBirth
    );
}
