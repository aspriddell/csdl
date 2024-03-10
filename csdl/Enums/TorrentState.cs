namespace csdl.Enums;

public enum TorrentState
{
    Unknown = 0,
    CheckingFiles = 1,
    CheckingResume = 2,
    DownloadingMetadata = 3,
    Downloading = 4,
    Seeding = 5,
    Finished = 6,
    Errored = 7
}
