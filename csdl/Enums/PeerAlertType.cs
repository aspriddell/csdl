// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

namespace csdl.Enums;

public enum PeerAlertType : byte
{
    ConnectedIncoming = 0,
    ConnectedOutgoing = 1,
    Disconnected = 2,
    Banned = 3,
    Snubbed = 4,
    Unsnubbed = 5,
    Errored = 6
}