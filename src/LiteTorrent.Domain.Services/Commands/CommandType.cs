namespace LiteTorrent.Domain.Services.InterProcessProtocol.Server.Commands;

public enum CommandType
{
    CreateSharedFile, // Create torrent file from raw file and add to distribution
    AddSharedFile, // Add existing torrent file to distribution
    GetAllSharedFiles, // Distribution status
    GetTorrentFile, // GetTorrentFile
    DeleteSharedFile
}