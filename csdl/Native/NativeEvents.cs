using System;
using System.Runtime.InteropServices;
using csdl.Enums;

namespace csdl.Native;

internal static class NativeEvents
{
    [StructLayout(LayoutKind.Sequential)]
    public struct EventBase
    {
        public int type;
        public int category;
        
        public long timestamp;
        
        [MarshalAs(UnmanagedType.LPStr)]
        public string message;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TorrentStatusAlert
    {
        [MarshalAs(UnmanagedType.Struct)]
        public EventBase info;

        public IntPtr handle;
        
        public TorrentState old_state;
        public TorrentState new_state;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PerformanceWarningAlert
    {
        [MarshalAs(UnmanagedType.Struct)]
        public EventBase info;
        
        public PerformanceWarningType warning_code;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PeerAlert
    {
        [MarshalAs(UnmanagedType.Struct)]
        public EventBase info;
        
        public IntPtr handle;
        public PeerAlertType alert_type;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] v6_address;
    }
}