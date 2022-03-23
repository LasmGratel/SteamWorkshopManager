using LiteDB;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Database;

public class UserDatabase
{
    private readonly ILiteDatabase _db;
    private readonly SteamWorkshopClient _client;

    private readonly ILiteCollection<UserEntry> _userCollection;

    public UserDatabase(ILiteDatabase db, SteamWorkshopClient client)
    {
        _db = db;
        _client = client;

        _userCollection = _db.GetCollection<UserEntry>("Users");

    }
    
}