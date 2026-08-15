// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using System;
using System.Net;
using csdl.Enums;
using csdl.Native;

namespace csdl.Alerts;

public class PeerAlert : SessionAlert
{
    internal PeerAlert(NativeEvents.PeerAlert alert, TorrentManager subject)
        : base(alert.info)
    {
        Subject = subject;
        AlertType = alert.alert_type;
        EndpointType = alert.endpoint_type;

        // Extract the appropriate address based on endpoint type
        if (alert.endpoint_type == EndpointType.IpEndpoint)
        {
            // IPv6 address (first 16 bytes)
            var ipv6Bytes = new byte[16];
            Array.Copy(alert.address, ipv6Bytes, 16);
            Address = new IPAddress(ipv6Bytes);
            I2pHash = null;
        }
        else
        {
            // I2P destination hash (full 32 bytes)
            Address = null;
            I2pHash = alert.address;
        }
    }

    public TorrentManager Subject { get; }

    public PeerAlertType AlertType { get; }
    public EndpointType EndpointType { get; }

    /// <summary>
    /// IP address for IP endpoints (null for I2P).
    /// </summary>
    public IPAddress? Address { get; }

    /// <summary>
    /// I2P destination hash for I2P endpoints (null for IP).
    /// </summary>
    public byte[]? I2pHash { get; }
}