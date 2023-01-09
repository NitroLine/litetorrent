using System.Text.Json.Serialization;

namespace LiteTorrent.Backend.Dto;

public record DtoSharedFile(
    [property: JsonPropertyName("hash")] string Hash,
    [property: JsonPropertyName("relativePath")] string RelativePath,
    [property: JsonPropertyName("downloadedPiecesPercentage")] double DownloadedPiecesPercentage
);