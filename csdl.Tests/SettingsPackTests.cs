// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using System;
using JetBrains.Annotations;

namespace csdl.Tests
{
    [TestSubject(typeof(SettingsPack))]
    public class SettingsPackTests : IDisposable
    {
        private readonly TorrentClient _client;

        public SettingsPackTests()
        {
            _client = new TorrentClient();
        }

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

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}