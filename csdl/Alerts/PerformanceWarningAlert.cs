// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using csdl.Enums;
using csdl.Native;

namespace csdl.Alerts;

public class PerformanceWarningAlert : SessionAlert
{
    internal PerformanceWarningAlert(NativeEvents.PerformanceWarningAlert alert)
        : base(alert.info)
    {
        WarningCode = alert.warning_code;
    }

    public PerformanceWarningType WarningCode { get; }
}