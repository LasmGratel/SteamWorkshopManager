using System.Collections.Generic;
using System.Threading.Tasks;
using LiteDB;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Database;

public class CacheDatabase
{
    private readonly ILiteDatabase _db;
    private readonly SteamWorkshopClient _client;

    private readonly ILiteCollection<Workshop> _appsCollection;

    public CacheDatabase(ILiteDatabase db, SteamWorkshopClient client)
    {
        _db = db;
        _client = client;

        _appsCollection = _db.GetCollection<Workshop>("Apps");
    }

    public async Task RefreshApps()
    {
        _appsCollection.DeleteAll();
        _appsCollection.InsertBulk(await _client.GetAllWorkshops());
    }

    public void UpdateTags(long appId, List<WorkshopItemTag> tags)
    {
        if (_appsCollection.FindById(appId) is { } workshop)
        {
            if (workshop.Tags.Count == 0)
            {
                workshop.Tags = tags;
                _appsCollection.Update(workshop);
            }
        }
    }

    public void UpdateApp(Workshop app)
    {
        _appsCollection.Update(app);
    }

    public async Task<Workshop?> GetApp(long appId)
    {
        if (_appsCollection.FindById(appId) is { } ret)
        {
            return ret;
        }

        await RefreshApps();
        return _appsCollection.FindById(appId);
    }

    public IEnumerable<Workshop> GetApps()
    {
        return _appsCollection.FindAll();
    }
}