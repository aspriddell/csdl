using System;
using System.Net;
using csdl.Enums;
using csdl.Native;

namespace csdl;

public class SessionAlert : EventArgs
{
    internal SessionAlert(NativeEvents.AlertBase alert)
    {
        Type = alert.type;
        Category = alert.category;
        Timestamp = DateTimeOffset.FromUnixTimeSeconds(alert.timestamp);
        Message = alert.message;
    }

    public AlertType Type { get; }
    
    public int Category { get; }
    
    public DateTimeOffset Timestamp { get; }

    public string Message { get; }
}

public class TorrentStatusAlert : SessionAlert
{
    internal TorrentStatusAlert(NativeEvents.TorrentStatusAlert alert, TorrentManager subjectManager)
        : base(alert.info)
    {
        Subject = subjectManager;
        OldState = alert.old_state;
        NewState = alert.new_state;
    }
    
    public TorrentManager Subject { get; }

    public TorrentState OldState { get; }
    public TorrentState NewState { get; }
}

public class TorrentRemovedAlert : SessionAlert
{
    internal TorrentRemovedAlert(NativeEvents.TorrentRemovedAlert alert, TorrentManager subject)
        : base(alert.info)
    {
        Subject = subject;
    }

    public TorrentManager Subject { get; }
}

public class PerformanceWarningAlert : SessionAlert
{
    internal PerformanceWarningAlert(NativeEvents.PerformanceWarningAlert alert)
        : base(alert.info)
    {
        WarningCode = alert.warning_code;
    }
    
    public PerformanceWarningType WarningCode { get; }
}

public class PeerAlert : SessionAlert
{
    internal PeerAlert(NativeEvents.PeerAlert alert, TorrentManager subject)
        : base(alert.info)
    {
        Subject = subject;
        AlertType = alert.alert_type;
        Address = new IPAddress(alert.v6_address);
    }
    
    public TorrentManager Subject { get; }

    public PeerAlertType AlertType { get; }
    public IPAddress Address { get; }
}