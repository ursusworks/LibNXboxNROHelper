using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ursus.Xbox.Models;

public sealed class TitleHistoryResponse
{
    [JsonPropertyName("titles")]
    public List<TitleItem> Titles { get; set; } = new();

    [JsonPropertyName("pagingInfo")]
    public PagingInfo? PagingInfo { get; set; }
}

public sealed class PagingInfo
{
    [JsonPropertyName("continuationToken")]
    public string? ContinuationToken { get; set; }

    [JsonPropertyName("totalRecords")]
    public int? TotalRecords { get; set; }
}

public sealed class TitleItem
{
    [JsonPropertyName("titleId")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public ulong TitleId { get; set; }

    [JsonPropertyName("modernTitleId")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public ulong? ModernTitleId { get; set; }

    [JsonPropertyName("pfn")]
    public string? Pfn { get; set; }

    // Localized name for the title
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    // e.g., "Game", "DGame", "App"
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    // e.g., ["XboxOne","XboxSeries","Windows10"]
    [JsonPropertyName("devices")]
    public List<string>? Devices { get; set; }

    [JsonPropertyName("displayImage")]
    public string? DisplayImage { get; set; }

    [JsonPropertyName("mediaId")]
    public string? MediaId { get; set; }

    [JsonPropertyName("isBundle")]
    public bool? IsBundle { get; set; }

    [JsonPropertyName("titleHistory")]
    public TitleHistoryInfo? TitleHistory { get; set; }

    [JsonPropertyName("playSummary")]
    public PlaySummary? PlaySummary { get; set; }

    [JsonPropertyName("achievement")]
    public AchievementSummary? Achievement { get; set; }

    // Present when using decoration=detail
    [JsonPropertyName("detail")]
    public TitleDetail? Detail { get; set; }

    // Preserve any unexpected fields
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; set; }
}

public sealed class TitleHistoryInfo
{
    [JsonPropertyName("firstTimePlayed")]
    public DateTimeOffset? FirstTimePlayed { get; set; }

    [JsonPropertyName("lastTimePlayed")]
    public DateTimeOffset? LastTimePlayed { get; set; }
}

public sealed class PlaySummary
{
    [JsonPropertyName("numTimesPlayed")]
    public int? NumTimesPlayed { get; set; }

    [JsonPropertyName("numFocus")]
    public int? NumFocus { get; set; }

    [JsonPropertyName("lastTimePlayed")]
    public DateTimeOffset? LastTimePlayed { get; set; }
}

public sealed class AchievementSummary
{
    [JsonPropertyName("currentAchievements")]
    public int? CurrentAchievements { get; set; }

    [JsonPropertyName("totalAchievements")]
    public int? TotalAchievements { get; set; }

    [JsonPropertyName("currentGamerscore")]
    public int? CurrentGamerscore { get; set; }

    [JsonPropertyName("totalGamerscore")]
    public int? TotalGamerscore { get; set; }

    [JsonPropertyName("progressPercentage")]
    public double? ProgressPercentage { get; set; }

    [JsonPropertyName("sourceVersion")]
    public int? SourceVersion { get; set; }

    [JsonPropertyName("areAchievementsOffline")]
    public bool? AreAchievementsOffline { get; set; }
}

public sealed class TitleDetail
{
    [JsonPropertyName("titleId")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public ulong? TitleId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("developerName")]
    public string? DeveloperName { get; set; }

    [JsonPropertyName("publisherName")]
    public string? PublisherName { get; set; }

    [JsonPropertyName("genres")]
    public List<string>? Genres { get; set; }

    [JsonPropertyName("images")]
    public List<TitleImage>? Images { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; set; }
}

public sealed class TitleImage
{
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

    [JsonPropertyName("purpose")]
    public string? Purpose { get; set; } // e.g., "BoxArt", "HeroArt"

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }
}