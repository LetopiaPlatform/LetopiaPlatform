using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LetopiaPlatform.Core.DTOs.Email;
public sealed record EmailChangeRequest(string NewEmail);

public sealed record EmailConfirmRequest(
    Guid UserId,
    string Token);
