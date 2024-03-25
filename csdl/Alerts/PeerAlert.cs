// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using System.Net;
using csdl.Enums;
using csdl.Native;

namespace csdl.Alerts;

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