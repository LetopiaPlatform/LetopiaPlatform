using System.Net;
using System.Net.Sockets;
using LetopiaPlatform.Core.Exceptions;

namespace LetopiaPlatform.Infrastructure.Services.Http;

/// <summary>
/// A <see cref="DelegatingHandler"/> that blocks connections to private,
/// loopback, and link-local IP ranges to prevent SSRF attacks.
/// </summary>
public sealed class SsrfBlockingHandler : DelegatingHandler
{
    private static readonly IReadOnlyList<(IPAddress Network, int PrefixLength)> BlockedRanges =
    [
        (IPAddress.Parse("10.0.0.0"),      8),   // RFC-1918 private
        (IPAddress.Parse("172.16.0.0"),   12),   // RFC-1918 private
        (IPAddress.Parse("192.168.0.0"),  16),   // RFC-1918 private
        (IPAddress.Parse("127.0.0.0"),     8),   // loopback
        (IPAddress.Parse("169.254.0.0"),  16),   // link-local (AWS metadata etc.)
        (IPAddress.Parse("::1"),         128),   // IPv6 loopback
        (IPAddress.Parse("fc00::"),        7),   // IPv6 ULA
        (IPAddress.Parse("fe80::"),       10),   // IPv6 link-local
    ];

    public SsrfBlockingHandler() : base(new SocketsHttpHandler
    {
        AllowAutoRedirect = false,
        ConnectTimeout = TimeSpan.FromSeconds(3),
    })
    { }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var uri = request.RequestUri
            ?? throw new ValidationException("Request URI is null.");

        await BlockIfInternalAsync(uri.Host, cancellationToken);

        return await base.SendAsync(request, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    private static async Task BlockIfInternalAsync(string host, CancellationToken cancellationToken)
    {
        var addresses = IPAddress.TryParse(host, out var parsed)
            ? [parsed]
            : await Dns.GetHostAddressesAsync(host, cancellationToken);

        if (addresses.Length == 0)
            throw new NotFoundException($"Could not resolve host: {host}");

        foreach (var ip in addresses)
        {
            var resolved = ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip;
            if (IsBlocked(resolved))
                throw new SsrfBlockedException(resolved.ToString());
        }
    }

    private static bool IsBlocked(IPAddress ip)
    {
        foreach ((IPAddress network, int prefix) in BlockedRanges)
        {
            if (network.AddressFamily != ip.AddressFamily) continue;
            if (IsInRange(ip, network, prefix)) return true;
        }
        return false;
    }

    private static bool IsInRange(IPAddress ip, IPAddress network, int prefixLength)
    {
        var ipBytes = ip.GetAddressBytes();
        var networkBytes = network.GetAddressBytes();

        var fullBytes = prefixLength / 8;
        var remainder = prefixLength % 8;

        for (var i = 0; i < fullBytes; i++)
            if (ipBytes[i] != networkBytes[i]) return false;

        if (remainder == 0) return true;

        var mask = (byte)(0xFF << (8 - remainder));
        return (ipBytes[fullBytes] & mask) == (networkBytes[fullBytes] & mask);
    }
}
