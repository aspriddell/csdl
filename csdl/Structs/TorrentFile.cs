using System;
using System.Diagnostics.CodeAnalysis;
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

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

/// <summary>
/// Represents a file contained within a torrent.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("ReSharper", "ConvertToAutoProperty")]
public class TorrentFile
{
    private int index;

    [MarshalAs(UnmanagedType.LPUTF8Str)]
    private string file_name;
    
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    private string file_path;

    private long file_size;

    public int Index => index;
    
    public string FileName => file_name;
    public string FilePath => file_path;
    
    public long FileSize => file_size;
}