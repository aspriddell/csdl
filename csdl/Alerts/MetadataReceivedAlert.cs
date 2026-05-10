// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using csdl.Native;

namespace csdl.Alerts;

public class MetadataReceivedAlert : SessionAlert
{
    internal MetadataReceivedAlert(NativeEvents.MetadataReceivedAlert alert, TorrentManager subject)
        : base(alert.info)
    {
        Subject = subject;
    }

    /// <summary>
    /// The torrent manager whose metadata has been received.
    /// <see cref="TorrentManager.Info"/> is now populated.
    /// </summary>
    public TorrentManager Subject { get; }
}
