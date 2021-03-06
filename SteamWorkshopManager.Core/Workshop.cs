using LiteDB;

namespace SteamWorkshopManager.Core;

public record Workshop
{
    public const int HeaderWidth = 460;
    public const int HeaderHeight = 215;

    [BsonId(false)]
    public long AppId { get; set; }

    public string Name { get; set; } = "No name";

    [BsonIgnore]
    public string Url => $"https://steamcommunity.com/app/{AppId}/workshop/";

    [BsonIgnore]
    public string ImageUrl => $"https://cdn.akamai.steamstatic.com/steam/apps/{AppId}/header.jpg";

    [BsonIgnore]
    public string CapsuleImageUrl => $"https://cdn.akamai.steamstatic.com/steam/apps/{AppId}/capsule_184x69.jpg";

    public List<WorkshopItemTag> Tags { get; set; } = new();

    public virtual bool Equals(Workshop? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return AppId == other.AppId;
    }

    public override int GetHashCode()
    {
        return AppId.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }
}