// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using System.Runtime.InteropServices;
using csdl.Enums;

namespace csdl;

/// <summary>
/// Contains information about the current state of an attached torrent.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct TorrentStatus
{
    public readonly TorrentState State;
    public readonly float Progress;

    public readonly int PeerCount;
    public readonly int SeedCount;

    public readonly long BytesUploaded;
    public readonly long BytesDownloaded;

    public readonly long UploadRate;
    public readonly long DownloadRate;
}