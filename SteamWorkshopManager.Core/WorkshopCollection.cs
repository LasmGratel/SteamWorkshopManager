namespace SteamWorkshopManager.Core;

public record WorkshopCollection
{
    public long Id { get; set; }

    public string? ImageUrl { get; set; }

    public string Name { get; set; } = "";

    public long AppId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdatedDate { get; set; }

    public string ShortDescription { get; set; } = "";

    public string? Description { get; set; }

    public long Visitors { get; set; }

    public long Subscribers { get; set; }

    public long Favorites { get; set; }

    public List<string> Tags { get; set; } = new();
}