// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

namespace csdl.Enums;

public enum AlertType
{
    Generic = 0,
    TorrentStatus = 1,
    ClientPerformance = 2,
    Peer = 3,
    TorrentRemoved = 4
}