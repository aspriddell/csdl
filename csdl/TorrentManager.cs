// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using System;
using System.Collections.Generic;
using System.Linq;
using csdl.Enums;
using csdl.Native;

namespace csdl;

public class TorrentManager
{
    private readonly string _savePath;
    internal readonly IntPtr TorrentSessionHandle;

    private bool _detached;
    private IReadOnlyList<TorrentManagerFile> _files;

    internal TorrentManager(IntPtr torrentSessionHandle, string savePath, TorrentInfo info)
    {
        Info = info;
        TorrentSessionHandle = torrentSessionHandle;

        _savePath = savePath;
    }

    /// <summary>
    /// Information about the .torrent file
    /// </summary>
    public TorrentInfo Info { get; }

    /// <summary>
    /// Information about the files contained within the torrent, with additional properties including file priorities and target save paths.
    /// </summary>
    public IReadOnlyList<TorrentManagerFile> Files => _files ??= Info.Files.Select(x => new TorrentManagerFile(TorrentSessionHandle, _savePath, x)).ToList();

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
    /// Reannounces the torrent to all trackers.
    /// </summary>
    /// <param name="interval">The delay between making this call and the announcement taking place</param>
    /// <param name="force">Whether to ignore any internal cooldowns between announcements</param>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="interval"/> was not valid</exception>
    public void ReannounceAllTrackers(TimeSpan interval, bool force = false)
    {
        if (interval.Seconds <= -1)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be a positive value.");
        }

        ObjectDisposedException.ThrowIf(_detached, this);
        NativeMethods.ReannounceTorrent(TorrentSessionHandle, (int)interval.TotalSeconds, force);
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