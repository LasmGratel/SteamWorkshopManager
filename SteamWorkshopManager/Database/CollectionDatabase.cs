using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Database;

public class CollectionDatabase
{
    private readonly ILiteDatabase _db;
    private readonly SteamWorkshopClient _client;

    private readonly ILiteCollection<WorkshopCollection> _collections;

    public CollectionDatabase(ILiteDatabase db, SteamWorkshopClient client)
    {
        _db = db;
        _client = client;

        _collections = _db.GetCollection<WorkshopCollection>("Collections");
    }

    public async Task<WorkshopCollection?> GetCollection(long id)
    {
        if (_collections.FindById(id) is { } ret)
            return ret;

        var result = await _client.GetMyCollectionsAsync();
        _collections.DeleteMany(x => result.Any(y => x.Id == y.Id));
        _collections.InsertBulk(result);

        return _collections.FindById(id);
    }

    public IEnumerable<WorkshopCollection> GetCollectionsForApp(long appId)
    {
        return _collections.Find(Query.EQ("AppId", appId));
    }

    public async Task<IEnumerable<WorkshopCollection>> GetAllCollections()
    {
        if (_collections.Count() == 0)
        {
            _collections.InsertBulk(await _client.GetMyCollectionsAsync());
        }

        return _collections.FindAll();
    }

    public async Task RefreshCollections()
    {
        foreach (var collection in await _client.GetMyCollectionsAsync())
        {
            InsertOrUpdate(collection);
        }
    }

    public long Insert(WorkshopCollection collection)
    {
        return _collections.Insert(collection);
    }

    public void InsertOrUpdate(WorkshopCollection collection)
    {
        _collections.Upsert(collection);
    }
}