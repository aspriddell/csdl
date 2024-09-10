// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using System;
using System.Net;
using csdl.Native;
using csdl.Utils;
using JetBrains.Annotations;

namespace csdl.Tests
{
    [TestSubject(typeof(SettingsPack))]
    public class SettingsPackTests : IDisposable
    {
        private readonly TorrentClient _client = new();

        [Fact]
        public void TestSettingsPack()
        {
            var pack = new SettingsPack();
            pack.Set("user_agent", "libtorrent/1.20");

            Assert.Equal("libtorrent/1.20", pack.Get("user_agent"));

            _client.UpdateSettings(pack);
        }

        [Fact]
        public void TestInvalidSettings()
        {
            var pack = new SettingsPack();
            pack.Set("invalid_data", 100);

            Assert.Throws<ArgumentException>(() => pack.Get<bool>("invalid_data"));
            Assert.Throws<ArgumentException>(() => pack.BuildNative());
        }

        [Fact]
        public void TestListenInterfaces()
        {
            var pack = new SettingsPack();
            var interfaces = new[]
            {
                new IPInterface(new IPEndPoint(IPAddress.IPv6Any, 6001)),
                new IPInterface(new IPEndPoint(IPAddress.Loopback, 10001), ListenInterface.ListenFlags.ListenSSL)
            };

            pack.UseListenInterfaces(interfaces);

            Assert.Equal("[::]:6001", interfaces[0].ToString());
            Assert.Equal("127.0.0.1:10001s", interfaces[1].ToString());
            Assert.Contains("[::]:6001", pack.Get("listen_interfaces"));

            // test native pack compatibility
            var nativePack = pack.BuildNative();
            NativeMethods.FreeSettingsPack(nativePack);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}