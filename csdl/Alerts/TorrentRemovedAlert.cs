// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using csdl.Native;

namespace csdl.Alerts;

public class TorrentRemovedAlert : SessionAlert
{
    internal TorrentRemovedAlert(NativeEvents.TorrentRemovedAlert alert, TorrentManager subject)
        : base(alert.info)
    {
        Subject = subject;
    }

    public TorrentManager Subject { get; }
}