// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using System;
using System.Runtime.InteropServices;
using csdl.Enums;

namespace csdl.Native;

internal static partial class NativeMethods
{
    private const string LibraryName = "csdl";

    /// <summary>
    /// Delegate representing the callback for session events.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SessionEventCallback(IntPtr alertPtr);

    /// <summary>
    /// Creates a session with the given configuration.
    /// </summary>
    /// <param name="config">The configuration to apply</param>
    /// <returns>A handle to the session</returns>
    [DllImport(LibraryName, EntryPoint = "create_session")]
    public static extern IntPtr CreateSession(in NativeStructs.SessionConfig config);

    /// <summary>
    /// Creates a session, optionally using a provided settings pack.
    /// </summary>
    /// <param name="settingsPack">A settings pack handle, set to <see cref="IntPtr.Zero"/> to initialise without customisation</param>
    /// <returns>A handle to the session</returns>
    [LibraryImport(LibraryName, EntryPoint = "create_session_from_pack")]
    public static unsafe partial IntPtr CreateSession(void* settingsPack);

    /// <summary>
    /// Releases the unmanaged resources associated with a session.
    /// </summary>
    /// <param name="sessionHandle">The session to invalidate</param>
    [LibraryImport(LibraryName, EntryPoint = "destroy_session")]
    public static partial void FreeSession(IntPtr sessionHandle);

    /// <summary>
    /// Sets the event callback for a session.
    /// </summary>
    /// <param name="sessionHandle">The handle for the session to add the callback to</param>
    /// <param name="callback">The callback to run when an event is posted</param>
    /// <param name="includeUnmappedEvents">Whether to run the <see cref="callback"/> for events that aren't mapped, that only produce <see cref="NativeEvents.AlertBase"/> values with no additional data</param>
    [LibraryImport(LibraryName, EntryPoint = "set_event_callback")]
    public static partial void SetEventCallback(IntPtr sessionHandle, [MarshalAs(UnmanagedType.FunctionPtr)] SessionEventCallback callback, [MarshalAs(UnmanagedType.Bool)] bool includeUnmappedEvents);

    /// <summary>
    /// Clears the currently set event callback.
    /// </summary>
    /// <param name="sessionHandle">The session handle to remove the registered callback from</param>
    [LibraryImport(LibraryName, EntryPoint = "clear_event_callback")]
    public static partial void ClearEventCallback(IntPtr sessionHandle);

    /// <summary>
    /// Applies a settings pack to a session.
    /// </summary>
    /// <param name="sessionHandle">The session handle to apply the pack to</param>
    /// <param name="settingsPack">The pack handle to apply</param>
    [LibraryImport(LibraryName, EntryPoint = "apply_settings")]
    public static partial void ApplySettingsPack(IntPtr sessionHandle, IntPtr settingsPack);

    /// <summary>
    /// Create a torrent from a file on the local disk
    /// </summary>
    /// <param name="path">The path to the file to parse</param>
    /// <returns>A handle to the torrent or <see cref="IntPtr.Zero"/> if there an error occurred</returns>
    [LibraryImport(LibraryName, EntryPoint = "create_torrent_file", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr CreateTorrentFromFile([MarshalAs(UnmanagedType.LPUTF8Str)] string path);

    /// <summary>
    /// Create a torrent from a magnet link
    /// </summary>
    /// <param name="magnet">The magnet link to parse</param>
    /// <returns>A handle to the torrent or <see cref="IntPtr.Zero"/> if there an error occurred</returns>
    [LibraryImport(LibraryName, EntryPoint = "create_torrent_magnet", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr CreateTorrentFromMagnet([MarshalAs(UnmanagedType.LPUTF8Str)] string magnet);

    /// <summary>
    /// Create a torrent file from a byte array
    /// </summary>
    /// <param name="content">The memory region containing the torrent file</param>
    /// <param name="length">The size of the region</param>
    /// <returns>A handle to the torrent or <see cref="IntPtr.Zero"/> if there an error occurred</returns>
    [LibraryImport(LibraryName, EntryPoint = "create_torrent_bytes")]
    public static partial IntPtr CreateTorrentFromBytes(byte[] content, long length);

    /// <summary>
    /// Create a torrent file from a byte array
    /// </summary>
    /// <param name="content">A byte pointer that locates the first byte of the torrent file</param>
    /// <param name="length">The size of the region</param>
    /// <returns>A handle to the torrent or <see cref="IntPtr.Zero"/> if there an error occurred</returns>
    [LibraryImport(LibraryName, EntryPoint = "create_torrent_bytes")]
    public static partial IntPtr CreateTorrentFromBytes(IntPtr content, long length);

    /// <summary>
    /// Releases the unmanaged resources associated with a torrent.
    /// </summary>
    /// <param name="torrentHandle">
    /// A handle to the torrent.
    /// This can be obtained from either <see cref="CreateTorrentFromFile"/> or <see cref="CreateTorrentFromBytes"/>
    /// </param>
    [LibraryImport(LibraryName, EntryPoint = "destroy_torrent")]
    public static partial void FreeTorrent(IntPtr torrentHandle);

    /// <summary>
    /// Attach a torrent to a session, allowing it to be downloaded
    /// </summary>
    /// <param name="sessionHandle">The session handle to attach the torrent to</param>
    /// <param name="torrentHandle">The handle of the torrent to attach</param>
    /// <param name="savePath">The path to save the contents of the torrent to</param>
    /// <returns>A torrent-session handle</returns>
    [LibraryImport(LibraryName, EntryPoint = "attach_torrent", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr AttachTorrent(IntPtr sessionHandle, IntPtr torrentHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string savePath);

    /// <summary>
    /// Detaches a torrent from a session, stopping the download.
    /// </summary>
    /// <param name="sessionHandle">The session handle to detach from</param>
    /// <param name="torrentSessionHandle">The torrent-session handle to detach</param>
    /// <remarks>
    /// After this method has returned, the <see cref="sessionHandle"/> has been released and is no longer valid for use.
    /// </remarks>
    [LibraryImport(LibraryName, EntryPoint = "detach_torrent")]
    public static partial void DetachTorrent(IntPtr sessionHandle, IntPtr torrentSessionHandle);

    /// <summary>
    /// Gets the torrent info from the torrent handle.
    /// </summary>
    /// <param name="torrentHandle">The handle of the torrent</param>
    /// <returns>A handle to a <see cref="NativeStructs.TorrentFile"/> struct (torrent metadata handle)</returns>
    /// <remarks>A call to FreeTorrentInfo is needed to release unmanaged resources after usage has finished.</remarks>
    [LibraryImport(LibraryName, EntryPoint = "get_torrent_info")]
    public static partial IntPtr GetTorrentInfo(IntPtr torrentHandle);

    /// <summary>
    /// Destroys a torrent info handle.
    /// </summary>
    /// <param name="torrentInfoHandle">The handle to release</param>
    [LibraryImport(LibraryName, EntryPoint = "destroy_torrent_info")]
    public static partial void FreeTorrentInfo(IntPtr torrentInfoHandle);

    /// <summary>
    /// Request a list of files contained within a torrent.
    /// </summary>
    /// <param name="torrentHandle">Handle of the torrent file to get info for</param>
    /// <param name="files">Location of the <see cref="NativeStructs.TorrentFileList"/> to populate</param>
    [LibraryImport(LibraryName, EntryPoint = "get_torrent_file_list")]
    public static partial void GetTorrentFileList(IntPtr torrentHandle, out NativeStructs.TorrentFileList files);

    /// <summary>
    /// Release the unmanaged resources associated with a <see cref="files"/>.
    /// </summary>
    /// <param name="files">The file list to release</param>
    [LibraryImport(LibraryName, EntryPoint = "destroy_torrent_file_list")]
    public static partial void FreeTorrentFileList(ref NativeStructs.TorrentFileList files);

    /// <summary>
    /// Get the download priority of a file within a torrent.
    /// </summary>
    /// <param name="torrentSessionHandle">The torrent session handle</param>
    /// <param name="fileIndex">The index of the file to get the priority of</param>
    [LibraryImport(LibraryName, EntryPoint = "get_file_dl_priority")]
    public static partial FileDownloadPriority GetFilePriority(IntPtr torrentSessionHandle, int fileIndex);

    /// <summary>
    /// Sets the download priority of a file within a torrent.
    /// </summary>
    /// <param name="torrentSessionHandle">The torrent session handle</param>
    /// <param name="fileIndex">The index of the file to set the priority of</param>
    /// <param name="priority">The download priority to apply</param>
    [LibraryImport(LibraryName, EntryPoint = "set_file_dl_priority")]
    public static partial void SetFilePriority(IntPtr torrentSessionHandle, int fileIndex, FileDownloadPriority priority);

    /// <summary>
    /// Starts or resumes a torrent download
    /// </summary>
    /// <param name="torrentSessionHandle">The torrent session handle to start</param>
    [LibraryImport(LibraryName, EntryPoint = "start_torrent")]
    public static partial void StartTorrent(IntPtr torrentSessionHandle);

    /// <summary>
    /// Stops a torrent download
    /// </summary>
    /// <param name="torrentSessionHandle">The torrent session handle to stop</param>
    [LibraryImport(LibraryName, EntryPoint = "stop_torrent")]
    public static partial void StopTorrent(IntPtr torrentSessionHandle);

    /// <summary>
    /// Re-announce the torrent to all trackers.
    /// </summary>
    /// <param name="torrentSessionHandle">The torrent session handle to apply the reannounce to</param>
    /// <param name="seconds">The delay, in seconds, before performing the reannouncement</param>
    /// <param name="force">Whether to ignore min times between reannounces</param>
    [LibraryImport(LibraryName, EntryPoint = "reannounce_torrent")]
    public static partial void ReannounceTorrent(IntPtr torrentSessionHandle, int seconds, [MarshalAs(UnmanagedType.I1)] bool force);

    /// <summary>
    /// Gets the status of a torrent.
    /// </summary>
    /// <param name="torrentSessionHandle">The torrent session handle to retrieve status information for</param>
    /// <param name="status">Variable to populate with the new state</param>
    [LibraryImport(LibraryName, EntryPoint = "get_torrent_status")]
    public static partial void GetTorrentStatus(IntPtr torrentSessionHandle, out TorrentStatus status);

    #region Settings Pack

    /// <summary>
    /// Creates an empty settings pack
    /// </summary>
    /// <remarks>
    /// This method is used to create a settings pack that can be customised and applied to a session.
    /// Add items using the SettingsPackAdd* methods, and apply it using either <see cref="ApplySettingsPack"/> or <see cref="CreateSession"/>.
    /// Note this handle needs to be manually freed, even after applying it to a session.
    /// </remarks>
    /// <returns>A handle to the created settings pack</returns>
    [LibraryImport(LibraryName, EntryPoint = "create_settings_pack")]
    public static partial IntPtr CreateSettingsPack();

    /// <summary>
    /// Frees a settings pack by its handle.
    /// </summary>
    /// <param name="settingsPack">The pack to free</param>
    [LibraryImport(LibraryName, EntryPoint = "destroy_settings_pack")]
    public static partial void FreeSettingsPack(IntPtr settingsPack);

    /// <summary>
    /// Adds an <see cref="int"/> value to the settings pack
    /// </summary>
    /// <param name="settingsPack">The pack handle</param>
    /// <param name="key">The configuration key to set</param>
    /// <param name="value">The integer value to apply</param>
    /// <returns>
    /// Whether the value was successfully added.
    /// If false, either the configuration does not exist or is the wrong type.
    /// </returns>
    [return: MarshalAs(UnmanagedType.I1)]
    [LibraryImport(LibraryName, EntryPoint = "settings_pack_set_int", StringMarshalling = StringMarshalling.Utf8)]
    public static partial bool SettingsPackSetInt(IntPtr settingsPack, string key, int value);

    /// <summary>
    /// Adds a <see cref="bool"/> value to the settings pack
    /// </summary>
    /// <param name="settingsPack">The pack handle</param>
    /// <param name="key">The configuration key to set</param>
    /// <param name="value">The boolean value to apply</param>
    /// <returns>
    /// Whether the value was successfully added.
    /// If false, either the configuration does not exist or is the wrong type.
    /// </returns>
    [return: MarshalAs(UnmanagedType.I1)]
    [LibraryImport(LibraryName, EntryPoint = "settings_pack_set_bool", StringMarshalling = StringMarshalling.Utf8)]
    public static partial bool SettingsPackSetBool(IntPtr settingsPack, string key, [MarshalAs(UnmanagedType.I1)] bool value);

    /// <summary>
    /// Adds a <see cref="string"/> to the settings pack
    /// </summary>
    /// <param name="settingsPack">The pack handle</param>
    /// <param name="key">The configuration key to set</param>
    /// <param name="value">The value to apply</param>
    /// <returns>
    /// Whether the value was successfully added.
    /// If false, either the configuration does not exist or is the wrong type.
    /// </returns>
    [return: MarshalAs(UnmanagedType.I1)]
    [LibraryImport(LibraryName, EntryPoint = "settings_pack_set_str", StringMarshalling = StringMarshalling.Utf8)]
    public static partial bool SettingsPackSetString(IntPtr settingsPack, string key, string value);

    #endregion
}