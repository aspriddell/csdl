// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using csdl.Alerts;
using csdl.Enums;
using JetBrains.Annotations;
using Xunit.Abstractions;

namespace csdl.Tests;

[TestSubject(typeof(TorrentClient))]
public class TorrentClientTests : IDisposable
{
    private readonly TorrentClient _client = new(new TorrentClientConfig
    {
        ForceEncryption = true,
        BlockSeeding = true
    });

    private readonly ITestOutputHelper _output;
    private readonly string _tempSavePath;

    public TorrentClientTests(ITestOutputHelper output)
    {
        _output = output;
        _tempSavePath = Path.Combine(Path.GetTempPath(), "csdl-test");

        Directory.CreateDirectory(_tempSavePath);
    }

    public void Dispose()
    {
        _client?.Dispose();
        Directory.Delete(_tempSavePath, true);
    }

    [Fact]
    public async Task TestTorrentDownload()
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

            // perform reannouncement
            torrentManager.ReannounceAllTrackers(TimeSpan.Zero);

            // check all files have been downloaded and are the correct size
            foreach (var file in torrentManager.Files.Where(x => x.Priority != FileDownloadPriority.DoNotDownload))
            {
                Assert.True(File.Exists(file.Path));
                Assert.Equal(file.Info.FileSize, new FileInfo(file.Path).Length);
            }
        }
        finally
        {
            await PerformCleanup(torrentManager);
        }

        Assert.True(!_client.ActiveTorrents.Contains(torrentManager));
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

    private async Task PerformCleanup(TorrentManager manager)
    {
        var cleanupTask = new TaskCompletionSource();

        _client.AlertRaised += CheckAlert;

        try
        {
            _client.DetachTorrent(manager);
            await cleanupTask.Task.WaitAsync(TimeSpan.FromSeconds(30));
        }
        catch
        {
            // log warning but don't fail
            _output.WriteLine("Failed to cleanup torrent manager in time. The event may not have been raised.");
        }
        finally
        {
            _client.AlertRaised -= CheckAlert;
        }

        return;

        void CheckAlert(object sender, SessionAlert alert)
        {
            if (alert is not TorrentRemovedAlert removedAlert || !ReferenceEquals(removedAlert.Subject, manager))
            {
                return;
            }

            foreach (var file in manager.Files)
            {
                try
                {
                    File.Delete(file.Path);
                }
                catch (Exception ex)
                {
                    // log the error but don't fail
                    _output.WriteLine($"Failed to delete file {file.Path}. Error: {ex.Message}");
                }
            }

            cleanupTask.TrySetResult();
        }
    }
}