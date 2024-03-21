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
}