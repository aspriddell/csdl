using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using csdl.Enums;
using JetBrains.Annotations;
using Xunit.Abstractions;

namespace csdl.Tests;

[TestSubject(typeof(TorrentClient))]
public class TorrentClientTests : IDisposable
{
    private readonly string _tempSavePath;
    private readonly ITestOutputHelper _output;
    private readonly TorrentClient _client = new(new TorrentClientConfig
    {
        ForceEncryption = true,
        BlockSeeding = true
    });
    
    public TorrentClientTests(ITestOutputHelper output)
    {
        _output = output;
        _tempSavePath = Path.Combine(Path.GetTempPath(), "csdl-test");
        
        Directory.CreateDirectory(_tempSavePath);
    }
    
    [Fact]
    public async Task TestTorrentDownloading()
    {
        var torrentInfo = new TorrentInfo(Path.GetFullPath(Path.Combine("files", "big-buck-bunny.torrent")));
        var torrentManager = _client.AttachTorrent(torrentInfo, _tempSavePath);
        
        var tcs = new TaskCompletionSource();
        
        // only download the non-video files (< 10mb)
        foreach (var file in torrentManager.Files.Where(x => x.Info.FileSize > 1e+7))
        {
            file.Priority = FileDownloadPriority.DoNotDownload;
        }

        try
        {
            torrentManager.Start();
            await using (new Timer(CheckProgress, (torrentManager, tcs), TimeSpan.Zero, TimeSpan.FromSeconds(5)))
            {
                await tcs.Task.WaitAsync(TimeSpan.FromMinutes(2));
            }
        }
        finally
        {
            torrentManager.Stop();
        }

        // check all files have been downloaded and are the correct size
        foreach (var file in torrentManager.Files.Where(x => x.Priority != FileDownloadPriority.DoNotDownload))
        {
            Assert.True(File.Exists(file.Path));
            Assert.Equal(file.Info.FileSize, new FileInfo(file.Path).Length);
        }
    }

    private void CheckProgress(object state)
    {
        var (manager, tcs) = (ValueTuple<TorrentManager, TaskCompletionSource>)state;
        var status = manager.GetCurrentStatus();

        if (status.State is TorrentState.Finished or TorrentState.Seeding)
        {
            tcs.TrySetResult();
        }

        _output.WriteLine($"Progress: {status.State} {status.Progress * 100:F2}% ({status.SeedCount:N0} seeds)");
    }

    public void Dispose()
    {
        _client?.Dispose();
        Directory.Delete(_tempSavePath, true);
    }
}