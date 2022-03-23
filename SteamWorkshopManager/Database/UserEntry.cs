using System.Collections.Generic;
using LiteDB;

namespace SteamWorkshopManager.Database;

public record UserEntry
{
    [BsonId]
    public long Id { get; set; }
    
    public List<long> SubscribedItems { get; set; }

    public List<long> FavoriteItems { get; set; }

    public List<long> Collections { get; set; }
}