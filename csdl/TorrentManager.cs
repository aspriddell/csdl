using System;
using System.Collections.Generic;
using System.Linq;
using csdl.Enums;
using csdl.Native;

namespace csdl;

public class TorrentManager
{
    internal readonly IntPtr TorrentSessionHandle;
    
    private readonly string _savePath;
    private IReadOnlyCollection<TorrentManagerFile> _files;

    private bool _detached;
    
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
    public IReadOnlyCollection<TorrentManagerFile> Files => _files ??= Info.Files.Select(x => new TorrentManagerFile(TorrentSessionHandle, _savePath, x)).ToList();

    /// <summary>
    /// Gets the current status of the torrent.
    /// </summary>
    public TorrentStatus Status
    {
        get
        {
            ObjectDisposedException.ThrowIf(_detached, this);
            Native.NativeMethods.GetTorrentStatus(TorrentSessionHandle, out var status);

            return status;
        }
    }
    
    /// <summary>
    /// Starts or resumes the torrent.
    /// </summary>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_detached, this);
        Native.NativeMethods.StartTorrent(TorrentSessionHandle);
    }
    
    /// <summary>
    /// Stops the torrent.
    /// </summary>
    public void Stop()
    {
        ObjectDisposedException.ThrowIf(_detached, this);
        Native.NativeMethods.StopTorrent(TorrentSessionHandle);
    }

    // internal method to trigger a detached status, essentially making the object functionally unusable.
    internal void MarkAsDetached()
    {
        _detached = true;
    }

    public class TorrentManagerFile
    {
        private readonly IntPtr _torrentSessionHandle;
        private readonly string _savePath;

        internal TorrentManagerFile(IntPtr torrentSessionHandle, string savePath, TorrentFileInfo info)
        {
            _torrentSessionHandle = torrentSessionHandle;
            _savePath = savePath;

            Info = info;
        }
        
        /// <summary>
        /// File information, as provided by the .torrent file.
        /// </summary>
        public TorrentFileInfo Info { get; }
        
        /// <summary>
        /// The full path to the file on disk.
        /// </summary>
        public string Path => Info.PathIsAbsolute ? Info.Path : System.IO.Path.Combine(_savePath, Info.Path);

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