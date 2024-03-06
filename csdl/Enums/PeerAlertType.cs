namespace csdl.Enums;

public enum PeerAlertType : byte
{
    ConnectedIncoming = 0,
    ConnectedOutgoing = 1,
    Disconnected = 2,
    Banned = 3,
    Snubbed = 4,
    Unsnubbed = 5,
    Errored = 6
}