using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace csdl;

/// <summary>
/// Represents a file contained within a torrent.
/// </summary>
public record TorrentFileInfo(int Index, string Name, string Path, long FileSize);

/// <summary>
/// Contains metadata related to a torrent file.
/// </summary>
public record TorrentMetadata(string Name, string Creator, string Comment, int TotalFiles, long TotalSize, DateTimeOffset CreatedAt);

/// <summary>
/// Represents a .torrent file.
/// </summary>
public class TorrentInfo
{
    internal readonly IntPtr Handle;

    private TorrentMetadata _metadata;
    private IReadOnlyCollection<TorrentFileInfo> _files;

    /// <summary>
    /// Creates a new instance of <see cref="TorrentInfo"/> using the contents of a .torrent file from disk.
    /// </summary>
    /// <param name="fileName">The path to the .torrent file</param>
    /// <exception cref="FileNotFoundException">The file was not found</exception>
    /// <exception cref="InvalidOperationException">The file could not be loaded</exception>
    public TorrentInfo(string fileName)
    {
        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException("The specified file does not exist.", fileName);
        }

        Handle = NativeMethods.CreateTorrentFromFile(fileName);

        if (Handle <= IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create torrent from file provided.");
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="TorrentInfo"/> using the contents of a .torrent file from memory.
    /// </summary>
    /// <param name="fileBytes">The contents of a .torrent file, as a span of bytes</param>
    /// <exception cref="InvalidOperationException">The provided data was invalid</exception>
    public TorrentInfo(Span<byte> fileBytes)
    {
        Handle = NativeMethods.CreateTorrentFromBytes(fileBytes, fileBytes.Length);

        if (Handle <= IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create torrent from bytes provided.");
        }
    }
    
    // as TorrentInfo is shared a lot, we're not providing a dispose method
    // and instead letting the garbage collector handle it
    ~TorrentInfo()
    {
        NativeMethods.FreeTorrent(Handle);
    }
    
    /// <summary>
    /// Gets metadata related to the torrent file.
    /// </summary>
    public TorrentMetadata Metadata => _metadata ??= GetInfo();
    
    /// <summary>
    /// Gets a list of files contained within the torrent.
    /// </summary>
    public IReadOnlyCollection<TorrentFileInfo> Files => _files ??= GetFiles();

    private TorrentMetadata GetInfo()
    {
        var infoHandle = NativeMethods.GetTorrentInfo(Handle);

        if (infoHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to retrieve torrent metadata.");
        }
        
        try
        {
            var info = Marshal.PtrToStructure<NativeStructs.TorrentMetadata>(infoHandle);
            return new TorrentMetadata(info.name,
                info.creator,
                info.comment,
                info.total_files,
                info.total_size,
                DateTimeOffset.FromUnixTimeSeconds(info.creation_epoch));
        }
        finally
        {
            NativeMethods.FreeTorrentInfo(infoHandle);
        }
    }

    private IReadOnlyCollection<TorrentFileInfo> GetFiles()
    {
        NativeMethods.GetTorrentFileList(Handle, out var list);

        try
        {
            var files = new List<TorrentFileInfo>(list.length);
            var size = Marshal.SizeOf<NativeStructs.TorrentFile>();

            for (var i = 0; i < list.length; i++)
            {
                var file = Marshal.PtrToStructure<NativeStructs.TorrentFile>(list.items + size * i);
                files.Add(new TorrentFileInfo(file.index, file.file_name, file.file_path, file.file_size));
            }

            return files;
        }
        finally
        {
            NativeMethods.FreeTorrentFileList(ref list);
        }
    }
}