using System.Runtime.InteropServices;

namespace csdl.Structs;

[StructLayout(LayoutKind.Sequential)]
public readonly struct TorrentFileInfo
{
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    public readonly string Name;

    [MarshalAs(UnmanagedType.LPUTF8Str)]
    public readonly string Author;

    [MarshalAs(UnmanagedType.LPUTF8Str)]
    public readonly string Comment;

    public readonly int TotalFiles;
    public readonly long TotalSize;

    public readonly long CreationEpoch;
}