using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using csdl.Native;

namespace csdl;

/// <summary>
/// Represents a file contained within a torrent.
/// </summary>
[DebuggerDisplay("{Path} ({FileSize} bytes)")]
public record TorrentFileInfo(int Index, long Offset, string Name, string Path, long FileSize, bool PathIsAbsolute, bool IsPadFile);

/// <summary>
/// Contains metadata related to a torrent file.
/// </summary>
[DebuggerDisplay("{Name}")]
public record TorrentMetadata(string Name, string Creator, string Comment, int TotalFiles, long TotalSize, DateTimeOffset CreatedAt, string InfoHash, string InfoHashV2);

/// <summary>
/// Represents a .torrent file.
/// </summary>
[DebuggerDisplay("{Metadata.Name} ({Files.Count} Files)")]
public class TorrentInfo
{
    internal readonly IntPtr InfoHandle;

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

        InfoHandle = NativeMethods.CreateTorrentFromFile(fileName);

        if (InfoHandle <= IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create torrent from file provided.");
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="TorrentInfo"/> using the contents of a .torrent file from memory.
    /// </summary>
    /// <param name="fileBytes">The contents of a .torrent file, as a block of memory</param>
    /// <exception cref="InvalidOperationException">The provided data was invalid</exception>
    public TorrentInfo(byte[] fileBytes)
    {
        InfoHandle = NativeMethods.CreateTorrentFromBytes(fileBytes, fileBytes.Length);

        if (InfoHandle <= IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create torrent from bytes provided.");
        }
    }
    
    // as TorrentInfo is shared a lot, we're not providing a dispose method
    // and instead letting the garbage collector handle it
    ~TorrentInfo()
    {
        NativeMethods.FreeTorrent(InfoHandle);
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
        var infoHandle = NativeMethods.GetTorrentInfo(InfoHandle);

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
                DateTimeOffset.FromUnixTimeSeconds(info.creation_epoch),
                info.info_hash_sha1.All(b => b == 0) ? null : Convert.ToHexString(info.info_hash_sha1),
                info.info_hash_sha256.All(b => b == 0) ? null : Convert.ToHexString(info.info_hash_sha256));
        }
        finally
        {
            NativeMethods.FreeTorrentInfo(infoHandle);
        }
    }

    private IReadOnlyCollection<TorrentFileInfo> GetFiles()
    {
        NativeMethods.GetTorrentFileList(InfoHandle, out var list);

        try
        {
            var files = new List<TorrentFileInfo>(list.length);
            var size = Marshal.SizeOf<NativeStructs.TorrentFile>();

            for (var i = 0; i < list.length; i++)
            {
                var nativeFile = Marshal.PtrToStructure<NativeStructs.TorrentFile>(list.items + (size * i));
                var fileInfo = new TorrentFileInfo(nativeFile.index,
                nativeFile.offset,
                nativeFile.file_name, 
                nativeFile.file_path,
                nativeFile.file_size,
                nativeFile.file_path_is_absolute,
                nativeFile.pad_file);
                
                files.Add(fileInfo);
            }

            return files;
        }
        finally
        {
            NativeMethods.FreeTorrentFileList(ref list);
        }
    }
}