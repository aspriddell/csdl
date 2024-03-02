using System.Runtime.InteropServices;
using csdl.Enums;

namespace csdl.Structs;

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