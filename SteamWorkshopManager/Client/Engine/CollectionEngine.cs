using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Client.Engine;

public class CollectionEngine : FetchEngine<WorkshopItem>
{
    public List<long> ItemList { get; set; }

    public CollectionEngine(SteamWorkshopClient client, EngineHandle? engineHandle, List<long> itemList) : base(client,
        engineHandle)
    {
        ItemList = itemList;
    }

    public override IAsyncEnumerator<WorkshopItem> GetAsyncEnumerator(
        CancellationToken cancellationToken = new CancellationToken())
    {
        return new CollectionEngineAsyncEnumerator(this, Client);
    }

    public class CollectionEngineAsyncEnumerator : AbstractFetchEngineAsyncEnumerator<WorkshopItem, CollectionEngine>
    {
        public int Index = 0;

        public CollectionEngineAsyncEnumerator(CollectionEngine fetchEngine, SteamWorkshopClient client) : base(fetchEngine, client)
        {
        }

        public override async ValueTask<bool> MoveNextAsync()
        {
            if (FetchEngine.EngineHandle.IsCancelled || FetchEngine.EngineHandle.IsCompleted || Index >= FetchEngine.ItemList.Count)
            {
                FetchEngine.EngineHandle.Complete();
                return false;
            }

            var id = FetchEngine.ItemList[Index++];
            if (await AppContext.ItemDatabase.GetItem(id) is { } item)
            {
                CurrentEntityEnumerator = Enumerable.Repeat(item as WorkshopItem, 1).GetEnumerator();
                return true;
            }

            FetchEngine.EngineHandle.Complete();
            return false;
        }
    }
}