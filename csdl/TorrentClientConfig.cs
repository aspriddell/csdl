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

    public int? MaxConnections { get; set; } = 200;

    public SettingsPack Build()
    {
        var pack = new SettingsPack();

        // user-agent
        if (!string.IsNullOrEmpty(UserAgent))
        {
            pack.Set("user_agent", UserAgent);
        }

        // fingerprint
        if (!string.IsNullOrEmpty(Fingerprint))
        {
            pack.Set("peer_fingerprint", Fingerprint);
        }

        // events
        pack.Set("alert_mask", (int)AlertCategories);

        pack.Set("anonymous_mode", PrivateMode);
        pack.Set("seeding_outgoing_connections", !BlockSeeding);

        if (MaxConnections.HasValue)
        {
            pack.Set("connections_limit", MaxConnections.Value);
        }

        if (ForceEncryption)
        {
            pack.Set("out_enc_policy", 0);
            pack.Set("in_enc_policy", 0);
        }

        return pack;
    }
}