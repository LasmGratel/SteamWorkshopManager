using LiteDB;

namespace SteamWorkshopManager.Core;

/// <summary>
/// A summary of a workshop item
/// </summary>
public partial record WorkshopItem
{
    [BsonId] public long Id { get; set; }

    public string ImageUrl { get; set; }

    public string Name { get; set; }

    public long AppId { get; set; }

    public DateTime? SubscribedDate { get; set; }

    public DateTime? LastUpdatedDate { get; set; }

    public bool Subscribed { get; set; }

    public bool Favorited { get; set; }

    [BsonIgnore]
    public string Url => $"https://steamcommunity.com/workshop/filedetails/?id={Id}";

    public virtual bool Equals(WorkshopItem? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id && AppId == other.AppId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, AppId);
    }
}