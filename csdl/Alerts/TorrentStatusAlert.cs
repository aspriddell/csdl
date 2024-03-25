using csdl.Enums;
using csdl.Native;

namespace csdl.Alerts;

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