using System.Text.Json.Serialization;

namespace LiteTorrent.Backend.Dto;

public record DtoCreateInfo(
    [property: JsonPropertyName("relativePath")] string RelativePath,
    [property: JsonPropertyName("shardMaxSizeInBytes")] uint ShardMaxSizeInBytes,
    [property: JsonPropertyName("outputFileAbsolutePath")] string OutputFileAbsolutePath
);