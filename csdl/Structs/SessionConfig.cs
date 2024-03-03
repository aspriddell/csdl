using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace csdl.Structs;

/// <summary>
/// Holds configuration information relating to a session.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("ReSharper", "ConvertToAutoProperty")]
public class SessionConfig
{
    [MarshalAs(UnmanagedType.LPStr)]
    private string user_agent;

    [MarshalAs(UnmanagedType.LPStr)]
    private string fingerprint;

    private bool private_mode;
    private bool block_seeding;
    private bool force_encryption;

    private int max_connections = 200;

    public string UserAgent
    {
        get => user_agent;
        set => user_agent = value;
    }

    public string Fingerprint
    {
        get => fingerprint;
        set => fingerprint = value;
    }
    
    public bool PrivateMode
    {
        get => private_mode;
        set => private_mode = value;
    }
    
    public bool BlockSeeding
    {
        get => block_seeding;
        set => block_seeding = value;
    }
    
    public bool ForceEncryption
    {
        get => force_encryption;
        set => force_encryption = value;
    }
    
    public int MaxConnections
    {
        get => max_connections;
        set => max_connections = value;
    }
}