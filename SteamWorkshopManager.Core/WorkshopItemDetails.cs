using LiteDB;

namespace SteamWorkshopManager.Core;

/// <summary>
/// Detailed information about workshop item, can only be fetched from workshop/filedetails/?id
/// </summary>
public record WorkshopItemDetails : WorkshopItem
{
    public string? Description { get; set; }

    /// <summary>
    /// File size, usually in Megabytes (MB).
    /// </summary>
    public string? FileSize { get; set; }

    /// <summary>
    /// How many change notes
    /// </summary>
    public long Revisions { get; set; }

    public List<SteamProfile> Authors { get; set; } = new();

    public long Visitors { get; set; }

    public long Subscribers { get; set; }

    public long Favorites { get; set; }

    public int Stars { get; set; }

    /// <summary>
    /// Collections that include this item
    /// </summary>
    public long Collections { get; set; }

    public List<long> Dependencies { get; set; } = new();

    public List<string> ScreenshotUrls { get; set; } = new();
}