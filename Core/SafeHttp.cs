using System.Net;
using System.Net.Sockets;

namespace KamnttraApi.Core;

// ═══════════════════════════════════════════════════════════════════════════
// OUTBOUND REQUESTS TO A URL A USER TYPED
//
// Anything that fetches a user-supplied URL (the SAP Link health check) can be pointed at the
// server's own network: localhost, the database, the cloud metadata service at 169.254.169.254.
// This client only connects to public addresses:
//   • the address is checked when the socket is opened (after DNS), so a hostname that resolves
//     to an internal IP — or changes its answer between a check and the connect — is still refused;
//   • redirects are not followed (a public page could bounce to an internal one);
//   • no proxy is used, so the check can't be routed somewhere unexpected.
// ═══════════════════════════════════════════════════════════════════════════
public static class SafeHttp
{
    public sealed class BlockedAddressException : HttpRequestException
    {
        public BlockedAddressException() : base("That address isn't allowed: it points at a private or internal network.") { }
    }

    public static HttpClient CreateClient(TimeSpan timeout)
    {
        var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            UseProxy          = false,
            ConnectTimeout    = timeout,
            ConnectCallback   = async (ctx, ct) =>
            {
                var addresses = await Dns.GetHostAddressesAsync(ctx.DnsEndPoint.Host, ct);
                var allowed   = addresses.Where(IsPublic).ToArray();
                if (allowed.Length == 0) throw new BlockedAddressException();

                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
                try
                {
                    await socket.ConnectAsync(allowed, ctx.DnsEndPoint.Port, ct);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch { socket.Dispose(); throw; }
            },
        };
        return new HttpClient(handler) { Timeout = timeout };
    }

    // http or https only, no credentials in the URL.
    public static bool TryParse(string? url, out Uri uri, out string error)
    {
        uri = null!; error = "";
        if (!Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var u) || (u.Scheme != Uri.UriSchemeHttp && u.Scheme != Uri.UriSchemeHttps))
        { error = "Only http and https addresses can be checked."; return false; }
        if (!string.IsNullOrEmpty(u.UserInfo)) { error = "The address can't contain a user name or password."; return false; }
        uri = u; return true;
    }

    public static bool IsPublic(IPAddress ip)
    {
        if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
        if (IPAddress.IsLoopback(ip) || ip.Equals(IPAddress.Any) || ip.Equals(IPAddress.IPv6Any) || ip.Equals(IPAddress.None)) return false;

        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6Multicast || ip.IsIPv6UniqueLocal) return false;
            return true;
        }

        var b = ip.GetAddressBytes();
        return !(b[0] == 0                                          // 0.0.0.0/8
              || b[0] == 10                                         // private
              || b[0] == 127                                        // loopback
              || (b[0] == 100 && b[1] >= 64 && b[1] <= 127)         // carrier-grade NAT
              || (b[0] == 169 && b[1] == 254)                       // link-local — cloud metadata lives here
              || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)          // private
              || (b[0] == 192 && b[1] == 168)                       // private
              || (b[0] == 192 && b[1] == 0 && b[2] == 0)            // IETF protocol assignments
              || (b[0] == 198 && (b[1] == 18 || b[1] == 19))        // benchmarking
              || b[0] >= 224);                                      // multicast and reserved
    }
}
