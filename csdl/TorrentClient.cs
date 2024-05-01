// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using csdl.Alerts;
using csdl.Enums;
using csdl.Native;

namespace csdl;

/// <summary>
/// Represents a client that can register and control torrents download/uploads
/// </summary>
public class TorrentClient : IDisposable
{
    private const AlertCategories RequiredCategories = AlertCategories.Status;

    private readonly ConcurrentDictionary<string, TorrentManager> _attachedManagers = new(StringComparer.OrdinalIgnoreCase);

    // need to keep a reference to the delegate to prevent GC invalidating it
    private readonly NativeMethods.SessionEventCallback _eventCallback;
    private readonly IntPtr _handle;

    private bool _disposed;
    private bool _includeUnmappedEvents;

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
            max_connections = config.MaxConnections,
            force_encryption = config.ForceEncryption,

            set_alert_flags = true,
            alert_flags = (int)(config.AlertCategories | RequiredCategories)
        });

        if (_handle <= IntPtr.Zero)
        {
            throw new InvalidOperationException($"Failed to create session ({_handle}.");
        }

        _eventCallback = ProxyRaisedEvent;
        NativeMethods.SetEventCallback(_handle, _eventCallback, true);
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

        _disposed = true;

        NativeMethods.ClearEventCallback(_handle);
        NativeMethods.FreeSession(_handle);

        GC.SuppressFinalize(this);
    }

    ~TorrentClient()
    {
        Dispose();
    }

    /// <summary>
    /// Event invoked when a session alert is raised.
    /// The underlying event collection system is unmanaged, and is started/shutdown on the first/last subscription to this event.
    /// </summary>
    public event EventHandler<SessionAlert> AlertRaised;

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

        if (_attachedManagers.ContainsKey(torrent.Metadata.InfoHash))
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
        _attachedManagers.TryAdd(manager.Info.Metadata.InfoHash, manager);

        return manager;
    }

    /// <summary>
    /// Detaches a torrent from the session, stopping any ongoing transfers.
    /// </summary>
    /// <param name="manager">The manager to detach</param>
    public void DetachTorrent(TorrentManager manager)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // the manager is fully removed via the alert callback (fires once the torrent is fully removed)
        if (!_attachedManagers.ContainsKey(manager.Info.Metadata.InfoHash))
        {
            throw new InvalidOperationException("Unable to detach torrent from session. Ensure the torrent is attached to this session.");
        }

        manager.Stop();
        NativeMethods.DetachTorrent(_handle, manager.TorrentSessionHandle);
    }

    /// <summary>
    /// Marshals raised unmanaged events to managed equivalents, and forwards them to the <see cref="AlertRaised"/> event.
    /// These events are raised and proxied by the unmanaged library, and are automatically destroyed once the callback returns.
    /// </summary>
    /// <param name="eventPtr">A <see cref="IntPtr"/> to the underlying event (see <c>event.h</c>)</param>
    private unsafe void ProxyRaisedEvent(IntPtr eventPtr)
    {
        if (eventPtr == IntPtr.Zero)
        {
            return;
        }

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
                if (!_attachedManagers.TryGetValue(Convert.ToHexString(statusAlert.info_hash), out var torrentSubject))
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
                if (!_attachedManagers.TryGetValue(Convert.ToHexString(peerAlert.info_hash), out var peerSubject))
                {
                    return;
                }

                forwardAlert = new PeerAlert(peerAlert, peerSubject);
                break;
            }

            case AlertType.TorrentRemoved:
            {
                var removedAlert = Marshal.PtrToStructure<NativeEvents.TorrentRemovedAlert>(eventPtr);
                if (!_attachedManagers.TryRemove(Convert.ToHexString(removedAlert.info_hash), out var manager))
                {
                    return;
                }

                // mark as detached to prevent further usage
                manager.MarkAsDetached();

                forwardAlert = new TorrentRemovedAlert(removedAlert, manager);
                break;
            }
        }

        if (forwardAlert == null)
        {
            return;
        }

        // the native library always invokes this from another thread
        AlertRaised?.Invoke(this, forwardAlert);
    }
}
