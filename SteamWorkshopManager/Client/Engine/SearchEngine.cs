using System.Collections.Generic;
using System.Threading;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Client.Engine;

public class SearchEngine : FetchEngine<WorkshopItem>
{
    public string SearchText { get; }
    public SortOptions SortOptions { get; }
    public int Trend { get; }

    public SearchEngine(SteamWorkshopClient client, EngineHandle? engineHandle = null, string searchText = "", SortOptions sortOptions = SortOptions.Trend, int trend = 7) : base(client, engineHandle)
    {
        SearchText = searchText;
        SortOptions = sortOptions;
        Trend = trend;
    }

    public override IAsyncEnumerator<WorkshopItem> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        return Client.SearchWorkshopItems(394360, SearchText, SortOptions, Trend);
    }
}