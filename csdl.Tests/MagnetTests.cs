// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using System;
using System.IO;
using System.Threading.Tasks;
using csdl.Alerts;
using JetBrains.Annotations;
using Xunit.Abstractions;

namespace csdl.Tests;

[TestSubject(typeof(TorrentClient))]
public class MagnetTests : IDisposable
{
    private readonly TorrentClient _client = new(new TorrentClientConfig
    {
        ForceEncryption = true,
        BlockSeeding = true
    });

    private readonly ITestOutputHelper _output;
    private readonly string _tempSavePath;

    // fixture-derived values so tests stay consistent with the on-disk .torrent
    private readonly string _bigBuckBunnyMagnet;
    private readonly string _bigBuckBunnyInfoHash;
    private readonly string _bigBuckBunnyName;
    private readonly int _bigBuckBunnyFileCount;
    private readonly long _bigBuckBunnyTotalSize;

    public MagnetTests(ITestOutputHelper output)
    {
        _output = output;
        _tempSavePath = Path.Combine(Path.GetTempPath(), "csdl-magnet-test");
        Directory.CreateDirectory(_tempSavePath);

        var fixture = new TorrentInfo(Path.GetFullPath(Path.Combine("files", "big-buck-bunny.torrent")));
        _bigBuckBunnyInfoHash = fixture.Metadata.InfoHash;
        _bigBuckBunnyMagnet = $"magnet:?xt=urn:btih:{_bigBuckBunnyInfoHash}&dn=Big+Buck+Bunny";
        _bigBuckBunnyName = fixture.Metadata.Name;
        _bigBuckBunnyFileCount = fixture.Metadata.TotalFiles;
        _bigBuckBunnyTotalSize = fixture.Metadata.TotalSize;
    }

    public void Dispose()
    {
        _client?.Dispose();
        Directory.Delete(_tempSavePath, true);
    }

    [Fact]
    public void AttachMagnet_InvalidUri_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => _client.AttachMagnet("not-a-magnet-uri", _tempSavePath));
    }

    [Fact]
    public void AttachMagnet_ValidUri_ReturnsManagerWithMatchingInfoHash()
    {
        var manager = _client.AttachMagnet(_bigBuckBunnyMagnet, _tempSavePath);

        Assert.Equal(_bigBuckBunnyInfoHash, manager.InfoHash, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void AttachMagnet_InfoIsNullBeforeMetadataFetched()
    {
        var manager = _client.AttachMagnet(_bigBuckBunnyMagnet, _tempSavePath);

        Assert.Null(manager.Info);
    }

    [Fact]
    public void AttachMagnet_FilesIsEmptyBeforeMetadataFetched()
    {
        var manager = _client.AttachMagnet(_bigBuckBunnyMagnet, _tempSavePath);

        Assert.Empty(manager.Files);
    }

    [Fact]
    public void AttachMagnet_DuplicateUri_ThrowsInvalidOperationException()
    {
        _client.AttachMagnet(_bigBuckBunnyMagnet, _tempSavePath);

        Assert.Throws<InvalidOperationException>(() => _client.AttachMagnet(_bigBuckBunnyMagnet, _tempSavePath));
    }

    [Fact]
    public void AttachMagnet_AppearsInActiveTorrents()
    {
        var manager = _client.AttachMagnet(_bigBuckBunnyMagnet, _tempSavePath);

        Assert.Contains(manager, _client.ActiveTorrents);
    }

    [Fact]
    public async Task DetachMagnet_BeforeMetadata_RemovesFromActiveTorrents()
    {
        var manager = _client.AttachMagnet(_bigBuckBunnyMagnet, _tempSavePath);
        var removedTcs = new TaskCompletionSource();

        _client.AlertRaised += (_, alert) =>
        {
            if (alert is TorrentRemovedAlert removed && ReferenceEquals(removed.Subject, manager))
            {
                removedTcs.TrySetResult();
            }
        };

        _client.DetachTorrent(manager);
        await removedTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.DoesNotContain(manager, _client.ActiveTorrents);
    }

    [Fact]
    public async Task TestMagnetMetadataFetch()
    {
        var manager = _client.AttachMagnet(_bigBuckBunnyMagnet, _tempSavePath);

        Assert.Null(manager.Info);
        Assert.Empty(manager.Files);

        manager.Start();

        await manager.WaitForMetadata(TimeSpan.FromMinutes(2));

        Assert.NotNull(manager.Info);
        Assert.NotEmpty(manager.Files);
        Assert.Equal(_bigBuckBunnyName, manager.Info.Metadata.Name);
        Assert.Equal(_bigBuckBunnyFileCount, manager.Info.Metadata.TotalFiles);
        Assert.Equal(_bigBuckBunnyTotalSize, manager.Info.Metadata.TotalSize);

        await CleanupAsync(manager);
    }

    private async Task CleanupAsync(TorrentManager manager)
    {
        var removedTcs = new TaskCompletionSource();

        _client.AlertRaised += CheckAlert;

        try
        {
            _client.DetachTorrent(manager);
            await removedTcs.Task.WaitAsync(TimeSpan.FromSeconds(30));
        }
        catch
        {
            _output.WriteLine("Cleanup timed out — torrent removed alert may not have fired.");
        }
        finally
        {
            _client.AlertRaised -= CheckAlert;
        }

        return;

        void CheckAlert(object _, SessionAlert alert)
        {
            if (alert is TorrentRemovedAlert removed && ReferenceEquals(removed.Subject, manager))
            {
                removedTcs.TrySetResult();
            }
        }
    }
}
