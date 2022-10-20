using System.Net;

namespace LiteTorrent.Sdk.Misc;

public static class IpHelper
{
    /// <summary>
    ///     Parses address=hostname:port into IPEndPoint.
    ///     Dns name will be resolved.
    /// </summary>
    /// <param name="address">hostname:port</param>
    public static async Task<IEnumerable<IPEndPoint>> ParseWithDnsResolving(string address)
    {
        var splitAddress = address.Split(':');
        var ipHostEntry = await Dns.GetHostEntryAsync(splitAddress[0]);

        return ipHostEntry.AddressList.Select(ip => new IPEndPoint(ip, int.Parse(splitAddress[0])));
    }
}