// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using System;
using System.Runtime.InteropServices;
using csdl.Enums;

namespace csdl.Native;

internal static class NativeEvents
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AlertBase
    {
        [MarshalAs(UnmanagedType.I4)]
        public AlertType type;

        public int category;
        public long timestamp;

        [MarshalAs(UnmanagedType.LPUTF8Str)]
        public string message;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TorrentStatusAlert
    {
        [MarshalAs(UnmanagedType.Struct)]
        public AlertBase info;

        public TorrentState old_state;
        public TorrentState new_state;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] info_hash;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TorrentRemovedAlert
    {
        [MarshalAs(UnmanagedType.Struct)]
        public AlertBase info;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] info_hash;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PerformanceWarningAlert
    {
        [MarshalAs(UnmanagedType.Struct)]
        public AlertBase info;

        public PerformanceWarningType warning_code;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PeerAlert
    {
        [MarshalAs(UnmanagedType.Struct)]
        public AlertBase info;

        public IntPtr handle;
        public PeerAlertType alert_type;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] info_hash;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] v6_address;
    }
}