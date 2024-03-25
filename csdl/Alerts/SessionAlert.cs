using System;
using csdl.Enums;
using csdl.Native;

namespace csdl.Alerts;

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