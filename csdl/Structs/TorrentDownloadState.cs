using System.Runtime.InteropServices;
using csdl.Enums;

namespace csdl.Structs;

/// <summary>
/// Contains information about the current state of an attached torrent.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct TorrentDownloadState
{
    public TorrentState State;

    public float Progress;

    public int PeerCount;
    public int SeedCount;

    public long BytesUploaded;
    public long BytesDownloaded;

    public long UploadRate;
    public long DownloadRate;
}