using LiteDB;

namespace SteamWorkshopManager.Core;

public record WorkshopCollection
{
    [BsonId(true)]
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

    public HashSet<long> Items { get; set; } = new();

    public bool IsOnline { get; set; }

    public virtual bool Equals(WorkshopCollection? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id && Name == other.Name && AppId == other.AppId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, AppId);
    }

    public override string ToString()
    {
        return Name;
    }
}