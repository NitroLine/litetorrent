using MessagePack;

namespace LiteTorrent.Domain.Services.StateRepository;

[MessagePackObject]
public record State([property: Key(0)] Hash DownloadingFileHash);