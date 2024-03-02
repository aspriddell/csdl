using System;
using System.Runtime.InteropServices;

namespace csdl.Structs;

/// <summary>
/// Represents a list of files contained within a torrent.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct TorrentFileList
{
    public readonly int Length;
    public readonly IntPtr Items;
}

/// <summary>
/// Represents a file contained within a torrent.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct TorrentFile
{
    public readonly int Index;

    [MarshalAs(UnmanagedType.LPUTF8Str)]
    public readonly string FileName;
    
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    public readonly string FilePath;

    public readonly long FileSize;
}