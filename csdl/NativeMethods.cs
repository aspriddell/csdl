using System;
using System.Runtime.InteropServices;
using csdl.Enums;
using csdl.Structs;

namespace csdl;

internal static partial class NativeMethods
{
    private const string LibraryName = "csdl";

    /// <summary>
    /// Creates a session with the given configuration.
    /// </summary>
    /// <param name="config">The configuration to apply</param>
    /// <returns>A handle to the session</returns>
    [DllImport(LibraryName, EntryPoint = "create_session")]
    public static extern IntPtr CreateSession(in SessionConfig config);

    /// <summary>
    /// Releases the unmanaged resources associated with a session.
    /// </summary>
    /// <param name="sessionHandle">The session to invalidate</param>
    [LibraryImport(LibraryName, EntryPoint = "destroy_session")]
    public static partial void FreeSession(IntPtr sessionHandle);
    
    /// <summary>
    /// Create a torrent from a file on the local disk
    /// </summary>
    /// <param name="path">The path to the file to parse</param>
    /// <returns>A handle to the torrent or <see cref="IntPtr.Zero"/> if there an error occurred</returns>
    [LibraryImport(LibraryName, EntryPoint = "create_torrent_file", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr CreateTorrentFromFile([MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    
    /// <summary>
    /// Create a torrent file from a byte array
    /// </summary>
    /// <param name="content">The byte array containing the torrent file</param>
    /// <param name="length">The size of the array</param>
    /// <returns>A handle to the torrent or <see cref="IntPtr.Zero"/> if there an error occurred</returns>
    [LibraryImport(LibraryName, EntryPoint = "create_torrent_bytes")]
    public static partial IntPtr CreateTorrentFromBytes(Span<byte> content, long length);
    
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
    /// <returns>A handle to a <see cref="TorrentFile"/> struct (torrent metadata handle)</returns>
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
    /// <param name="files">Location of the <see cref="TorrentFileList"/> to populate</param>
    [LibraryImport(LibraryName, EntryPoint = "get_torrent_file_list")]
    public static partial void GetTorrentFileList(IntPtr torrentHandle, out NativeStructs.TorrentFileList files);
    
    /// <summary>
    /// Release the unmanaged resources associated with a <see cref="TorrentFileList"/>.
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
    [LibraryImport(LibraryName, EntryPoint = "start_dl")]
    public static partial void StartTorrent(IntPtr torrentSessionHandle);
    
    /// <summary>
    /// Stops a torrent download
    /// </summary>
    /// <param name="torrentSessionHandle">The torrent session handle to stop</param>
    [LibraryImport(LibraryName, EntryPoint = "stop_dl")]
    public static partial void StopTorrent(IntPtr torrentSessionHandle);
    
    /// <summary>
    /// Gets the status of a torrent.
    /// </summary>
    /// <param name="torrentSessionHandle">The torrent session handle to retrieve status information for</param>
    /// <param name="status">Variable to populate with the new state</param>
    [LibraryImport(LibraryName, EntryPoint = "get_torrent_status")]
    public static partial void GetTorrentStatus(IntPtr torrentSessionHandle, out TorrentStatus status);
}