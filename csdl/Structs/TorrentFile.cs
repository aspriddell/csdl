using System;
using System.Runtime.InteropServices;

namespace csdl.Structs;

/// <summary>
/// Represents a list of files contained within a torrent.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct TorrentFileList
{
    public int Length;
    public IntPtr Items;
}

/// <summary>
/// Represents a file contained within a torrent.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct TorrentFile
{
    public int Index;

    [MarshalAs(UnmanagedType.LPUTF8Str)]
    public string FileName;
    
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    public string FilePath;

    public long FileSize;
}