# csdl
Providing libtorrent through a C++ library for use in .NET

## Usage
[![Latest Nuget](https://img.shields.io/nuget/v/csdl.Native?label=csdl&logo=nuget)](https://nuget.org/packages/csdl)

Add `csdl` to the project via [NuGet](https://nuget.org/packages/csdl) and create a single `TorrentClient` instance.
This will usually be `static` (or singleton if using a dependency container), and a `TorrentClientConfig` can be passed in the constructor to configure the client.

```csharp
// create a new TorrentClient instance, optionally passing in the configuration
var options = new TorrentClientConfig
{
    ForceEncryption = true,
    MaxConnections = 500
};

using var client = new TorrentClient(options);

// bonus: there is also an event handler that can be subscribed to if more information is wanted.
client.AlertRaised += (sender, args) =>
{
    // args can be checked against all classes in the csdl.Alerts namespace for more properties.
    Console.WriteLine(args.Message);
};
```

.torrent files can be parsed either by passing in a file path or a byte array containing the file contents to the `TorrentInfo` class, and the instance will be populated accordingly.
Some metadata can be accessed from the `TorrentInfo` instance, such as the name of the torrent and a list of files contained within it.

```csharp
var filePath = "path/to/torrent/file.torrent";
var torrentInfo = new TorrentInfo(filePath);

// get the name and list of files
Console.WriteLine($"Name: {torrentInfo.Name}");
Console.WriteLine("Files:");

foreach (var file in torrentInfo.Files)
{
    Console.WriteLine($"- {file.Path} ({file.Size} bytes)");
}
```

After parsing a torrent file, it can be "attached" to the client to start downloading the files. Note this method will not start a download, but will prepare the client to download the files when `Start` is called.
This method also has an overload allowing a custom save path to be specified. If not, the default save path will be used (`client.DefaultDownloadPath`).

```csharp
// this will be saved to DefaultDownloadPath.
var torrentManager = client.AttachTorrent(torrentInfo);

// the metadata can still be accessed but files have additional properties including their final destination and their download priority, which can be changed.
torrentManager.Files[0].Priority = TorrentFilePriority.DoNotDownload;

// after setting priorities, the download can begin (or resume if the torrent was previously started)
torrentManager.Start();

// if we want a progress update, we can request one
var progress = torrentManager.GetCurrentStatus();

if (progress.State == TorrentState.Finished)
{
    torrentManager.Stop();

    // when we want to "dispose" the manager, we can detach it from the client
    client.DetachTorrent(torrentManager);
}
```

If we want to wait for the download to complete, a timer and a `TaskCompletionSource` can be used to await the completion of the download.

```csharp
void PerformDownload(TorrentClient client, TorrentInfo info, string savePath = null)
{
    var torrentTransfer = new TaskCompletionSource();
    var torrentManager = client.AttachTorrent(info, savePath);
    
    torrentManager.Start();
    
    using (new Timer(PerformProgressCheck, null, TimeSpan.Zero, TimeSpan.FromSeconds(1)))
    {
        await torrentTransfer.Task;
    }

    // at this point, the torrentManager has either finished downloading or seeding, and the poll has been stopped.

    client.DetachTorrent(torrentManager);
    return;

    // this function polls the progress (makes a call to libtorrent) every second to check if the torrent has finished downloading
    void PerformProgressCheck(object state)
    {
        if (torrentManager.GetCurrentStatus().State is TorrentState.Seeding or TorrentState.Finished)
        {
            torrentTransfer.SetResult();
        }
    }
}
```

### Supported Systems
The native libraries, `csdl.Native`, are currently built for Windows, macOS and Linux for both x64 and arm64 architectures.

Additionally, libtorrent is not required to be installed, a copy is also redistributed for convenience.

> [!IMPORTANT]
> musl Linux variants of the native libraries are not built but should work if built manually.
