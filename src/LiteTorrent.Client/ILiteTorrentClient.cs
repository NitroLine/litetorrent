namespace LiteTorrent.Client;

public interface ILiteTorrentClient
{
    IDistributorClient Distributor { get; }
    IDownloaderClient Downloader { get; }
}