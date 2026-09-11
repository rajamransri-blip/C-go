using System.Text.Json.Serialization;

namespace KuronamiGfx;

public class ConfigItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("badge_text")]
    public string BadgeText { get; set; } = "BGMI ONLY";

    [JsonPropertyName("badge_num")]
    public string BadgeNum { get; set; } = "10";

    [JsonPropertyName("file_url")]
    public string FileUrl { get; set; } = string.Empty;

    [JsonPropertyName("file_name")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("target_subpath")]
    public string TargetSubpath { get; set; } = "files/UE4Game/ShadowTrackerExtra/ShadowTrackerExtra/Saved/Paks";

    public bool IsDownloaded { get; set; }
}
