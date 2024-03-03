using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace csdl.Structs;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("ReSharper", "ConvertToAutoProperty")]
public class TorrentFileInfo
{
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    private string name;

    [MarshalAs(UnmanagedType.LPUTF8Str)]
    private string author;

    [MarshalAs(UnmanagedType.LPUTF8Str)]
    private string comment;

    private int total_files;
    private long total_size;

    private long creation_epoch;
    
    public string Name => name;
    public string Author => author;
    public string Comment => comment;
    
    public int TotalFiles => total_files;
    public long TotalSize => total_size;
    
    public DateTimeOffset CreatedAt => DateTimeOffset.FromUnixTimeSeconds(creation_epoch);
}