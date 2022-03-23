using System.Collections.Generic;
using System.Threading.Tasks;
using LiteDB;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Database;

public class WorkshopItemDatabase
{
    private readonly ILiteDatabase _db;
    private readonly SteamWorkshopClient _client;

    private readonly ILiteCollection<WorkshopItem> _subscribedItemCollection;
    private readonly ILiteCollection<WorkshopItemDetails> _detailsCollection;

    public WorkshopItemDatabase(ILiteDatabase db, SteamWorkshopClient client)
    {
        _db = db;
        _client = client;

        _subscribedItemCollection = _db.GetCollection<WorkshopItem>("SubscribedItems");
        _detailsCollection = _db.GetCollection<WorkshopItemDetails>("ItemDetails");

    }

    /// <summary>
    /// Update an item in the database
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public void UpdateItem(WorkshopItemDetails item)
    {
        _detailsCollection.Delete(new BsonValue(item.Id));
        _detailsCollection.Insert(new BsonValue(item.Id), item);
    }

    /// <summary>
    /// Update an item in the database
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task Update(long id)
    {
        var newDetail = await _client.GetWorkshopItemDetailsAsync(id);
        UpdateItem(newDetail);
    }

    /// <summary>
    /// Get an item from database, or update it if not exists
    /// </summary>
    /// <param name="id">Workshop item id</param>
    /// <returns>Workshop item details</returns>
    public async Task<WorkshopItemDetails> GetItem(long id)
    {
        if (_detailsCollection.FindById(new BsonValue(id)) is { } item)
        {
            return item;
        }
        else
        {
            var detail = await _client.GetWorkshopItemDetailsAsync(id);
            UpdateItem(detail);
            return detail;
        }
    }

    public void UpdateItems()
    {

    }

    public async IAsyncEnumerable<WorkshopItemDetails> GetSubscribedItems(int appId)
    {
        await foreach (var item in _client.GetAllSubscribedItems(appId))
        {
            var id = new BsonValue(item.Id);
            if (_subscribedItemCollection.FindById(id) is { } old)
            {
                if (old.LastUpdatedDate != item.LastUpdatedDate)
                    await Update(item.Id);
            }
            yield return await GetItem(item.Id);
        }
    }
}