using LiteDB;

namespace SteamWorkshopManager.Core;

/// <summary>
/// A summary of a workshop item
/// </summary>
public partial record WorkshopItem
{
    [BsonId]
    public long Id { get; set; }

    public string ImageUrl { get; set; }

    public string Name { get; set; }

    public long AppId { get; set; }

    public DateTime? SubscribedDate { get; set; }

    public DateTime? LastUpdatedDate { get; set; }

    [BsonIgnore]
    public string Url => $"https://steamcommunity.com/workshop/filedetails/?id={Id}";
}