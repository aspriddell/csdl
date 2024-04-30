// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using System;
using System.Runtime.InteropServices;

namespace csdl.Native;

internal static class NativeStructs
{
    /// <summary>
    /// Holds configuration information relating to a session.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SessionConfig
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string user_agent;

        [MarshalAs(UnmanagedType.LPStr)]
        public string fingerprint;

        public bool private_mode;
        public bool block_seeding;
        public bool force_encryption;

        public bool set_alert_flags;
        public uint alert_flags;

        public int max_connections;
    }

    /// <summary>
    /// Represents a file contained within a torrent.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct TorrentFile
    {
        public readonly int index;

        public readonly long offset;
        public readonly long file_size;

        public readonly long modified_time;

        [MarshalAs(UnmanagedType.LPUTF8Str)]
        public readonly string file_name;

        [MarshalAs(UnmanagedType.LPUTF8Str)]
        public readonly string file_path;

        public readonly bool file_path_is_absolute;
        public readonly bool pad_file;
    }

    /// <summary>
    /// Represents a list of files contained within a torrent.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct TorrentFileList
    {
        public readonly int length;
        public readonly IntPtr items;
    }

    /// <summary>
    /// Represents a single file contained within a torrent.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct TorrentMetadata
    {
        [MarshalAs(UnmanagedType.LPUTF8Str)]
        public readonly string name;

        [MarshalAs(UnmanagedType.LPUTF8Str)]
        public readonly string creator;

        [MarshalAs(UnmanagedType.LPUTF8Str)]
        public readonly string comment;

        public readonly int total_files;
        public readonly long total_size;

        public readonly long creation_epoch;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] info_hash_sha1;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] info_hash_sha256;
    }
}