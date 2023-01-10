namespace LiteTorrent.Domain.Services.LocalStorage.Configuration;

public record LocalStorageConfiguration(
    string PieceDirectoryPath,
    string HashTreeDirectoryPath,
    string SharedFileDirectoryPath
)
{
    public string InPieceDir(string path) => Path.Join(PieceDirectoryPath, path);
    
    public string InHashTreeDir(string path) => Path.Join(HashTreeDirectoryPath, path);
    
    public string InSharedFileDir(string path) => Path.Join(SharedFileDirectoryPath, path);
}