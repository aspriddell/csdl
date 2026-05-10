// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using csdl.Enums;
using csdl.Native;

namespace csdl;

#nullable enable

public class TorrentManager
{
    internal readonly IntPtr TorrentSessionHandle;

    private readonly string _savePath;
    private readonly TaskCompletionSource? _metadataTaskSrc;

    private bool _detached;
    private IReadOnlyList<TorrentManagerFile>? _files;

    internal TorrentManager(IntPtr torrentSessionHandle, string savePath, string infoHash, TorrentInfo? info)
    {
        Info = info;
        InfoHash = infoHash;
        TorrentSessionHandle = torrentSessionHandle;

        _savePath = savePath;
        _metadataTaskSrc = info == null ? new TaskCompletionSource() : null;
    }

    /// <summary>
    /// The v1 info-hash of the torrent, always available even before metadata is fetched for magnet links.
    /// </summary>
    public string InfoHash { get; }

    /// <summary>
    /// Information about the torrent.
    /// For magnet links, this is <c>null</c> until the metadata has been populated.
    /// </summary>
    public TorrentInfo? Info { get; private set; }

    /// <summary>
    /// Whether to automatically pause the torrent once metadata has been fetched, giving the caller a chance to
    /// configure file priorities before content downloading begins. Defaults to <c>true</c>.
    /// (only relevant for magnet links as the metadata is available instantly with standard torrent files)
    /// </summary>
    public bool PauseAfterMetadata { get; set; } = true;

    /// <summary>
    /// Information about the files contained within the torrent, with additional properties including file priorities and target save paths.
    /// Returns an empty list for magnet links that haven't had their metadata fetched yet.
    /// </summary>
    public IReadOnlyList<TorrentManagerFile> Files
    {
        get
        {
            if (Info == null)
            {
                return [];
            }

            return _files ??= Info.Files.Select(x => new TorrentManagerFile(TorrentSessionHandle, _savePath, x)).ToList();
        }
    }

    /// <summary>
    /// Gets the current status of the torrent.
    /// </summary>
    /// <remarks>
    /// Every time this is called, an unmanaged call is made to the underlying system.
    /// Where possible, cache the result of this method to avoid unnecessary overhead.
    /// </remarks>
    public TorrentStatus GetCurrentStatus()
    {
        ObjectDisposedException.ThrowIf(_detached, this);
        NativeMethods.GetTorrentStatus(TorrentSessionHandle, out var status);

        return status;
    }

    /// <summary>
    /// Starts or resumes the torrent.
    /// </summary>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_detached, this);
        NativeMethods.StartTorrent(TorrentSessionHandle);
    }

    /// <summary>
    /// Stops the torrent.
    /// </summary>
    public void Stop()
    {
        ObjectDisposedException.ThrowIf(_detached, this);
        NativeMethods.StopTorrent(TorrentSessionHandle);
    }

    /// <summary>
    /// Waits for the torrent metadata to be populated, using a <see cref="CancellationToken"/> as a timeout mechanism.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the waiting</param>
    public async Task WaitForMetadata(CancellationToken cancellationToken)
    {
        var task = _metadataTaskSrc?.Task ?? Task.CompletedTask;
        await task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Waits for the torrent metadata to be populated, using a <see cref="TimeSpan"/> as a timeout mechanism.
    /// </summary>
    /// <param name="timeout">How long to wait for the task to complete</param>
    public async Task WaitForMetadata(TimeSpan timeout)
    {
        var task = _metadataTaskSrc?.Task ?? Task.CompletedTask;
        await task.WaitAsync(timeout).ConfigureAwait(false);
    }

    /// <summary>
    /// Reannounces the torrent to all trackers.
    /// </summary>
    /// <param name="interval">The delay between making this call and the announcement taking place</param>
    /// <param name="force">Whether to ignore any internal cooldowns between announcements</param>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="interval"/> was not valid</exception>
    public void ReannounceAllTrackers(TimeSpan interval, bool force = false)
    {
        if (Math.Sign((int)interval.TotalSeconds) == -1)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be a positive value.");
        }

        ObjectDisposedException.ThrowIf(_detached, this);
        NativeMethods.ReannounceTorrent(TorrentSessionHandle, (int)interval.TotalSeconds, force);
    }

    // internal method to set the torrent info and signal to any event handlers the info is available.
    internal void OnMetadataReceived(TorrentInfo info)
    {
        Info = info;

        _files = null;
        _metadataTaskSrc?.TrySetResult();
    }

    // internal method to trigger a detached status, essentially making the object functionally unusable.
    internal void MarkAsDetached()
    {
        _detached = true;
    }

    public class TorrentManagerFile
    {
        private readonly IntPtr _torrentSessionHandle;

        internal TorrentManagerFile(IntPtr torrentSessionHandle, string savePath, TorrentFileInfo info)
        {
            _torrentSessionHandle = torrentSessionHandle;

            Info = info;
            Path = System.IO.Path.IsPathRooted(Info.Path) ? Info.Path : System.IO.Path.Combine(savePath, Info.Path);
        }

        /// <summary>
        /// File information, as provided by the .torrent file.
        /// </summary>
        public TorrentFileInfo Info { get; }

        /// <summary>
        /// The full path to the file on disk.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The download priority of the file.
        /// </summary>
        public FileDownloadPriority Priority
        {
            get => NativeMethods.GetFilePriority(_torrentSessionHandle, Info.Index);
            set => NativeMethods.SetFilePriority(_torrentSessionHandle, Info.Index, value);
        }
    }
}