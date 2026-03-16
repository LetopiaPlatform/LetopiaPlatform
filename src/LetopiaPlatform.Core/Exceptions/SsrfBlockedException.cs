using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LetopiaPlatform.Core.Exceptions;
public sealed class SsrfBlockedException : Exception
{
    public SsrfBlockedException(string host)
        : base($"Connections to internal/private addresses are not allowed: {host}") { }
}
