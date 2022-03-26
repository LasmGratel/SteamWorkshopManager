using LiteDB;

namespace SteamWorkshopManager.Core;

/// <summary>
/// A summary of a workshop item
/// </summary>
public partial record WorkshopItem
{
    [BsonId] public long Id;

    public string ImageUrl;

    public string Name;

    public long AppId;

    public DateTime? SubscribedDate;

    public DateTime? LastUpdatedDate;

    public bool Subscribed;

    public bool Favorited;

    [BsonIgnore]
    public string Url => $"https://steamcommunity.com/workshop/filedetails/?id={Id}";
}