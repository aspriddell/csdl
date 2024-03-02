namespace csdl.Enums;

public enum TorrentState
{
    CheckingFiles = 0,
    DownloadingMetadata = 1,
    Downloading = 2,
    Finished = 3,
    Seeding = 4,
    CheckingResumeData = 7
}
