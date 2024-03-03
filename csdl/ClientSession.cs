using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using csdl.Structs;

namespace csdl;

/// <summary>
/// Represents a torrent client that can register torrents
/// </summary>
public class ClientSession : IDisposable
{
    private readonly IntPtr _handle;
    private readonly ConcurrentDictionary<IntPtr, TorrentManager> _attachedManagers = new();

    public ClientSession()
        : this(new SessionConfig())
    {
    }
    
    public ClientSession(SessionConfig config)
    {
        _handle = NativeMethods.CreateSession(config);

        if (_handle <= IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create session.");
        }
    }
    
    ~ClientSession()
    {
        Dispose();
    }
    
    /// <summary>
    /// Gets or sets the default path to save downloaded torrents to.
    /// If a torrent is attached with a relative save path and this property is set, the save path will be combined with this property.
    /// </summary>
    public string DefaultDownloadPath { get; set; } = Path.Combine(Environment.CurrentDirectory, "downloads");
    
    /// <summary>
    /// Gets the active torrents currently attached to the session.
    /// </summary>
    public IEnumerable<TorrentManager> ActiveTorrents => _attachedManagers.Values;

    /// <summary>
    /// Attaches a torrent to the session, allowing it to be downloaded/uploaded.
    /// </summary>
    /// <param name="torrent">The <see cref="TorrentInfo"/> to attach</param>
    /// <param name="savePath">The path to save/read data from</param>
    /// <returns>A <see cref="TorrentManager"/> allowing the torrent to be controlled.</returns>
    /// <exception cref="InvalidOperationException">The torrent was unable to be attached to the underlying session</exception>
    public TorrentManager Attach(TorrentInfo torrent, string savePath = null)
    {
        savePath ??= DefaultDownloadPath;

        // relative paths will be combined with the default download path
        if (Path.IsPathRooted(savePath))
        {
            savePath = Path.Combine(DefaultDownloadPath, savePath);
        }
        
        // ensure the save path exists
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        
        var handle = NativeMethods.AttachTorrent(_handle, torrent.Handle, savePath);
        
        if (handle <= IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to attach torrent to session.");
        }
        
        var manager = new TorrentManager(handle, savePath, torrent);
        _attachedManagers.TryAdd(handle, manager);
        
        return manager;
    }

    /// <summary>
    /// Detaches a torrent from the session, stopping any ongoing transfers.
    /// </summary>
    /// <param name="manager">The manager to detach</param>
    public void Detach(TorrentManager manager)
    {
        if (!_attachedManagers.TryRemove(manager.TorrentSessionHandle, out _))
        {
            throw new InvalidOperationException("Unable to detach torrent from session. Ensure the torrent is attached to this session.");
        }
        
        manager.Stop();
        NativeMethods.DetachTorrent(_handle, manager.TorrentSessionHandle);
        
        // mark as disposed/detached to stop usage of any remaining references to the manager
        manager.MarkAsDetached();
    }

    public void Dispose()
    {
        foreach (var session in ActiveTorrents)
        {
            Detach(session);
        }
        
        NativeMethods.FreeSession(_handle);
        GC.SuppressFinalize(this);
    }
}