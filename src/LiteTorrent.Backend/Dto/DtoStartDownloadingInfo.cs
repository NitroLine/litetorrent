using System.Text.Json.Serialization;

namespace LiteTorrent.Backend.Dto;

public record DtoStartDownloadingInfo(
    [property: JsonPropertyName("hosts")] string[] Hosts, // DON'T CHANGE PROPERTIES ORDER
    [property: JsonPropertyName("hashBase32")] string HashBase32); // DON'T RENAME TO "hash"