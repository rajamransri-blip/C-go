using System.Text.Json.Serialization;

namespace KuronamiGfx;

public class ConfigItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("file_url")]
    public string FileUrl { get; set; } = string.Empty;

    [JsonPropertyName("file_name")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("target_subpath")]
    public string TargetSubpath { get; set; } = string.Empty;

    public bool IsDownloaded { get; set; }
}
