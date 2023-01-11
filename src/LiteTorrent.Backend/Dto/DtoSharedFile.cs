using System.Text.Json.Serialization;

namespace LiteTorrent.Backend.Dto;

public record DtoSharedFile(
    [property: JsonPropertyName("hash")] string HashBase32,
    [property: JsonPropertyName("relativePath")] string RelativePath,
    [property: JsonPropertyName("sizeInBytes")] ulong SizeInBytes,
    [property: JsonPropertyName("downloadedCount")] ulong DownloadedPieceCount,
    [property: JsonPropertyName("totalCount")] ulong TotalPieceCount,
    [property: JsonPropertyName("isDownloading")] bool IsDownloading
);