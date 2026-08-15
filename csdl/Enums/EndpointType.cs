// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

namespace csdl.Enums;

/// <summary>
/// Type of peer endpoint (IP or I2P).
/// </summary>
public enum EndpointType : byte
{
    IpEndpoint = 0,
    I2pEndpoint = 1
}
