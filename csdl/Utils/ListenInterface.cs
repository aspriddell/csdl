// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using System;
using System.Net;

namespace csdl.Utils;

/// <summary>
/// The base class for a listenable interface used for accepting incoming connections.
/// </summary>
public abstract record ListenInterface(ListenInterface.ListenFlags Flags)
{
    public enum ListenFlags
    {
        None,
        ListenSSL,
        ListenLocalNetwork
    }

    internal string GetFlagValue() => Flags switch
    {
        ListenFlags.ListenSSL => "s",
        ListenFlags.ListenLocalNetwork => "l",

        _ => string.Empty
    };
}

/// <summary>
/// Represents a network interface to listen for incoming connections on (by adapter id, windows only)
/// </summary>
public record GuidInterface(Guid Interface, ListenInterface.ListenFlags Flags = ListenInterface.ListenFlags.None) : ListenInterface(Flags)
{
    public override string ToString() => $"{Interface.ToString("B").ToUpperInvariant()}{GetFlagValue()}";
}

/// <summary>
/// Represents an IP-port combo that can be listened on for incoming connections
/// </summary>
public record IPInterface(IPEndPoint Endpoint, ListenInterface.ListenFlags Flags = ListenInterface.ListenFlags.None) : ListenInterface(Flags)
{
    public override string ToString() => $"{Endpoint}{GetFlagValue()}";
}