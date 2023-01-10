using System.Text.Json.Serialization;

namespace LiteTorrent.Backend.Dto;

public record DtoTorrentFile([property: JsonPropertyName("fileFullName")] string FileFullName);