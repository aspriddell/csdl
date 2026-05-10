// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace csdl.Tests;

public class TorrentInfoTests
{
    [Theory]
    [InlineData("big-buck-bunny.torrent", "Big Buck Bunny", 3)]
    [InlineData("ubuntu-20.04.6-live-server-amd64.iso.torrent", "ubuntu-20.04.6-live-server-amd64.iso", 1)]
    public async Task TestTorrentParsing(string fileName, string expectedName, int expectedFileCount)
    {
        var file = Path.GetFullPath(Path.Combine("files", fileName));
        var torrentBytes = await File.ReadAllBytesAsync(file);

        var torrentFromFile = new TorrentInfo(file);
        var torrentFromBytes = new TorrentInfo(torrentBytes);

        // name check
        Assert.Equal(expectedName, torrentFromFile.Metadata.Name);

        // metadata check
        Assert.Equal(torrentFromFile.Metadata, torrentFromBytes.Metadata);

        // file count
        Assert.Equal(expectedFileCount, torrentFromFile.Files.Count);

        // file equality
        var fromFileNames = torrentFromFile.Files.Select(x => x.Path).ToHashSet();
        var fromBytesNames = torrentFromBytes.Files.Select(x => x.Path).ToHashSet();

        Assert.True(fromFileNames.SetEquals(fromBytesNames));
    }

    [Theory]
    [InlineData("big-buck-bunny.torrent")]
    [InlineData("ubuntu-20.04.6-live-server-amd64.iso.torrent")]
    public void GetBytes_ReturnsNonEmptyBuffer(string fileName)
    {
        var torrent = new TorrentInfo(Path.GetFullPath(Path.Combine("files", fileName)));
        var bytes = torrent.GetBytes();

        Assert.NotNull(bytes);
        Assert.NotEmpty(bytes);
    }

    [Theory]
    [InlineData("big-buck-bunny.torrent", "Big Buck Bunny", 3)]
    [InlineData("ubuntu-20.04.6-live-server-amd64.iso.torrent", "ubuntu-20.04.6-live-server-amd64.iso", 1)]
    public void GetBytes_RoundtripPreservesMetadata(string fileName, string expectedName, int expectedFileCount)
    {
        var original = new TorrentInfo(Path.GetFullPath(Path.Combine("files", fileName)));
        var roundtrip = new TorrentInfo(original.GetBytes());

        Assert.Equal(expectedName, roundtrip.Metadata.Name);
        Assert.Equal(expectedFileCount, roundtrip.Files.Count);
        Assert.Equal(original.Metadata, roundtrip.Metadata);
    }

    [Theory]
    [InlineData("big-buck-bunny.torrent", "Big Buck Bunny")]
    [InlineData("ubuntu-20.04.6-live-server-amd64.iso.torrent", "ubuntu-20.04.6-live-server-amd64.iso")]
    public void SaveToFile_WritesReadableFile(string fileName, string expectedName)
    {
        var original = new TorrentInfo(Path.GetFullPath(Path.Combine("files", fileName)));
        var tempPath = Path.GetTempFileName();

        try
        {
            original.SaveToFile(tempPath);

            Assert.True(new FileInfo(tempPath).Length > 0);

            var loaded = new TorrentInfo(tempPath);
            Assert.Equal(expectedName, loaded.Metadata.Name);
            Assert.Equal(original.Metadata, loaded.Metadata);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }
}