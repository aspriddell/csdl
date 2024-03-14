using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using csdl.Enums;
using csdl.Native;

namespace csdl;

/// <summary>
/// Represents a client that can register and control torrents download/uploads
/// </summary>
public class TorrentClient : IDisposable
{
    private readonly IntPtr _handle;
    private readonly ConcurrentDictionary<IntPtr, TorrentManager> _attachedManagers = new();

    private bool _disposed;
    private bool _includeUnmappedEvents;

    // need to keep a reference to the delegate to prevent GC invalidating it
    private NativeMethods.SessionEventCallback _eventCallback;
    private EventHandler<SessionAlert> _alertEvent;
    private int _alertSubscriptionCount;

    public TorrentClient()
        : this(new TorrentClientConfig())
    {
    }

    public TorrentClient(TorrentClientConfig config)
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

    ~TorrentClient()
    {
        Dispose();
    }

    /// <summary>
    /// Event invoked when a session alert is raised.
    /// The underlying event collection system is unmanaged, and is started/shutdown on the first/last subscription to this event.
    /// </summary>
    public event EventHandler<SessionAlert> AlertRaised
    {
        add
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _alertEvent += value;

            if (Interlocked.Increment(ref _alertSubscriptionCount) != 1)
            {
                return;
            }

            _eventCallback = ProxyRaisedEvent;
            NativeMethods.SetEventCallback(_handle, _eventCallback, _includeUnmappedEvents);
        }
        remove
        {
            _alertEvent -= value;

            if (Interlocked.Decrement(ref _alertSubscriptionCount) != 0)
            {
                return;
            }

            _eventCallback = null;

            if (!_disposed)
            {
                NativeMethods.ClearEventCallback(_handle);
            }
        }
    }
    
    /// <summary>
    /// Gets the active torrents currently attached to the session.
    /// </summary>
    public IEnumerable<TorrentManager> ActiveTorrents => _attachedManagers.Values;
    
    /// <summary>
    /// Whether to include events that only produce a <see cref="SessionAlert"/> with no additional data.
    /// </summary>
    /// <remarks>
    /// Changing this value after subscribing will cause the event callback to be reset.
    /// </remarks>
    public bool IncludeUnmappedEvents
    {
        get => _includeUnmappedEvents;
        set
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _includeUnmappedEvents = value;

            // reset event callback if set
            if (_eventCallback != null)
            {
                NativeMethods.ClearEventCallback(_handle);
                NativeMethods.SetEventCallback(_handle, _eventCallback, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the default path to save downloaded torrents to.
    /// If a torrent is attached with a relative save path and this property is set, the save path will be combined with this property.
    /// </summary>
    public string DefaultDownloadPath { get; set; } = Path.Combine(Environment.CurrentDirectory, "downloads");

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
        if (!Path.IsPathRooted(savePath))
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

    /// <summary>
    /// Marshals raised unmanaged events to managed equivalents, and forwards them to the <see cref="AlertRaised"/> event.
    /// These events are raised and proxied by the unmanaged library, and are automatically destroyed once the callback returns.
    /// </summary>
    /// <param name="eventPtr">A <see cref="IntPtr"/> to the underlying event (see <c>event.h</c>)</param>
    private unsafe void ProxyRaisedEvent(IntPtr eventPtr)
    {
        SessionAlert forwardAlert = null;

        switch ((AlertType)(*(int*)eventPtr.ToPointer()))
        {
            case AlertType.Generic:
            {
                var genericAlert = Marshal.PtrToStructure<NativeEvents.AlertBase>(eventPtr);
                forwardAlert = new SessionAlert(genericAlert);
                break;
            }

            case AlertType.TorrentStatus:
            {
                var statusAlert = Marshal.PtrToStructure<NativeEvents.TorrentStatusAlert>(eventPtr);
                if (!_attachedManagers.TryGetValue(statusAlert.handle, out var torrentSubject))
                {
                    return;
                }

                forwardAlert = new TorrentStatusAlert(statusAlert, torrentSubject);
                break;
            }

            case AlertType.ClientPerformance:
            {
                var performanceAlert = Marshal.PtrToStructure<NativeEvents.PerformanceWarningAlert>(eventPtr);
                forwardAlert = new PerformanceWarningAlert(performanceAlert);
                break;
            }

            case AlertType.Peer:
            {
                var peerAlert = Marshal.PtrToStructure<NativeEvents.PeerAlert>(eventPtr);
                if (!_attachedManagers.TryGetValue(peerAlert.handle, out var peerSubject))
                {
                    return;
                }

                forwardAlert = new PeerAlert(peerAlert, peerSubject);
                break;
            }
        }

        if (forwardAlert == null)
        {
            return;
        }

        _alertEvent.Invoke(this, forwardAlert);
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
        _eventCallback = null;
    }
}