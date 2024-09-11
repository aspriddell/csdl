// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using System.Collections.Generic;
using System.Text;

namespace csdl.Utils;

public static class ListenInterfaceExtensions
{
    /// <summary>
    /// Writes a collection of listen interfaces to the provided <see cref="SettingsPack"/>
    /// </summary>
    public static SettingsPack UseListenInterfaces(this SettingsPack pack, IEnumerable<ListenInterface> interfaces)
    {
        var stringBuilder = new StringBuilder();
        foreach (var iface in interfaces)
        {
            stringBuilder.Append(iface);
            stringBuilder.Append(',');
        }

        // stop if nothing was written
        if (stringBuilder.Length <= 0)
        {
            return pack;
        }

        stringBuilder.Length--; // remove trailing comma
        pack.Set("listen_interfaces", stringBuilder.ToString());

        return pack;
    }
}
