using System.Runtime.InteropServices;

namespace csdl.Structs;

/// <summary>
/// Holds configuration information relating to a session.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct SessionConfig
{
    public SessionConfig()
    {
        UserAgent = null;
        Fingerprint = null;

        PrivateMode = false;
        BlockSeeding = false;
        ForceEncryption = true;

        MaxConnections = 200;
    }

    [MarshalAs(UnmanagedType.LPStr)]
    public string UserAgent;

    [MarshalAs(UnmanagedType.LPStr)]
    public string Fingerprint;

    public bool PrivateMode;
    public bool BlockSeeding;
    public bool ForceEncryption;

    public int MaxConnections;
}