using System.Collections.Generic;
using SteamWorkshopManager.Client.Engine;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Model;

public class WorkshopItemFetchEngineIncrementalSource : FetchEngineIncrementalSource<WorkshopItem, WorkshopItemViewModel>
{
    public WorkshopItemFetchEngineIncrementalSource(IAsyncEnumerable<WorkshopItem> asyncEnumerator, int? limit = null) : base(asyncEnumerator, limit)
    {
    }

    protected override long Identifier(WorkshopItem entity)
    {
        return entity!.Id;
    }

    protected override WorkshopItemViewModel Select(WorkshopItem entity)
    {
        return new WorkshopItemViewModel(entity);
    }
}