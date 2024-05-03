// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using csdl.Enums;

namespace csdl;

public class TorrentClientConfig
{
    public string UserAgent { get; set; }

    public string Fingerprint { get; set; }

    public bool PrivateMode { get; set; }
    public bool BlockSeeding { get; set; }
    public bool ForceEncryption { get; set; }

    /// <summary>
    /// The alert categories to enable events for.
    /// Some types of alerts are always enabled to ensure the client functions correctly.
    /// </summary>
    public AlertCategories AlertCategories { get; set; }

    public int MaxConnections { get; set; } = 200;
}