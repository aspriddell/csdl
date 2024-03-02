using System.Runtime.InteropServices;

namespace csdl.Structs;

[StructLayout(LayoutKind.Sequential)]
public struct TorrentFileInfo
{
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    public string Name;

    [MarshalAs(UnmanagedType.LPUTF8Str)]
    public string Author;

    [MarshalAs(UnmanagedType.LPUTF8Str)]
    public string Comment;

    public int TotalFiles;
    public long TotalSize;

    public long CreationEpoch;
}