using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using csdl.Native;

namespace csdl;

/// <summary>
/// Represents a torrent client that can register torrents
/// </summary>
public class ClientSession : IDisposable
{
    private readonly IntPtr _handle;
    private readonly ConcurrentDictionary<IntPtr, TorrentManager> _attachedManagers = new();

    private bool _disposed;
    
    public ClientSession()
        : this(new ClientSessionConfig())
    {
    }
    
    public ClientSession(ClientSessionConfig config)
    {
        _handle = NativeMethods.CreateSession(new NativeStructs.SessionConfig
        {
            user_agent = config.UserAgent,
            fingerprint = config.Fingerprint,
            private_mode = config.PrivateMode,
            block_seeding = config.BlockSeeding,
            force_encryption = config.ForceEncryption,
            max_connections = config.MaxConnections
        });

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
    public TorrentManager AttachTorrent(TorrentInfo torrent, string savePath = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        if (_attachedManagers.ContainsKey(torrent.InfoHandle))
        {
            throw new InvalidOperationException("Torrent is already attached to this session.");
        }
        
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
        
        var handle = NativeMethods.AttachTorrent(_handle, torrent.InfoHandle, savePath);
        
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
    public void DetachTorrent(TorrentManager manager)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
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
        if (_disposed)
        {
            return;
        }
        
        foreach (var session in ActiveTorrents)
        {
            try
            {
                DetachTorrent(session);
            }
            catch
            {
                // ignore
            }
        }
        
        NativeMethods.FreeSession(_handle);
        GC.SuppressFinalize(this);

        _disposed = true;
    }
}