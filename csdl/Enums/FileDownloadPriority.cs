// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

namespace csdl.Enums;

public enum FileDownloadPriority : byte
{
    DoNotDownload = 0,
    Low = 1,
    Normal = 4,
    High = 7
}