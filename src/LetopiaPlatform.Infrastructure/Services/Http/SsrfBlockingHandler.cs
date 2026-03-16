using System.Net;
using System.Net.Sockets;
using LetopiaPlatform.Core.Exceptions;

namespace LetopiaPlatform.Infrastructure.Services.Http;

/// <summary>
/// A <see cref="DelegatingHandler"/> that blocks connections to private,
/// loopback, and link-local IP ranges to prevent SSRF attacks.
/// Blocking is enforced at the TCP connect step via <see cref="SocketsHttpHandler.ConnectCallback"/>
/// so it fires after DNS resolution — catching cases that DNS-level checks miss.
/// </summary>
public sealed class SsrfBlockingHandler : DelegatingHandler
{
    public SsrfBlockingHandler() : base(new SocketsHttpHandler
    {
        AllowAutoRedirect = false,
        ConnectTimeout = TimeSpan.FromSeconds(3),
        ConnectCallback = async (context, ct) =>
        {
            var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, ct);

            if (addresses.Length == 0)
                throw new SocketException((int)SocketError.HostNotFound);

            // Unwrap IPv4-mapped IPv6 (::ffff:x.x.x.x) then filter blocked ranges
            var safeAddresses = addresses
                .Select(ip => ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip)
                .Where(ip => !IsPrivateOrReservedIp(ip))
                .ToArray();

            if (safeAddresses.Length == 0)
                throw new SsrfBlockedException("All resolved IP addresses are blocked.");

            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            try
            {
                await socket.ConnectAsync(safeAddresses, context.DnsEndPoint.Port, ct);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }
    })
    { }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
        => base.SendAsync(request, cancellationToken);

    private static bool IsPrivateOrReservedIp(IPAddress ip)
    {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            var b = ip.GetAddressBytes();
            return b[0] == 10                                            // 10.0.0.0/8
                || b[0] == 127                                           // 127.0.0.0/8  loopback
                || b[0] == 0                                             // 0.0.0.0/8    reserved
                || (b[0] == 100 && b[1] >= 64 && b[1] <= 127)          // 100.64.0.0/10 CGNAT
                || (b[0] == 169 && b[1] == 254)                         // 169.254.0.0/16 link-local
                || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)           // 172.16.0.0/12
                || (b[0] == 192 && b[1] == 168);                        // 192.168.0.0/16
        }

        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (IPAddress.IsLoopback(ip) || ip.Equals(IPAddress.IPv6Any)) return true;

            var b = ip.GetAddressBytes();
            return (b[0] & 0xFE) == 0xFC                                // fc00::/7  ULA
                || (b[0] == 0xFE && (b[1] & 0xC0) == 0x80)             // fe80::/10 link-local
                || (b[0] == 0x20 && b[1] == 0x01
                    && b[2] == 0x00 && b[3] == 0x00);                   // 2001:db8::/32 documentation
        }

        return false;
    }
}
