using System.Net;

namespace LiteTorrent.Sdk.TorrentFileEntities;

public record TorrentFile(IPEndPoint[] TrackerAddresses, DistributingFileInfo[] FileInfos);