using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Client.Engine;

public class SearchEngine : FetchEngine<WorkshopItem>
{
    public long AppId { get; }
    public string SearchText { get; }
    public SortOptions SortOptions { get; }
    public int Trend { get; }

    public SearchEngine(SteamWorkshopClient client, EngineHandle? engineHandle, long appId, string searchText = "", SortOptions sortOptions = SortOptions.Trend, int trend = 7) : base(client, engineHandle)
    {
        SearchText = searchText;
        SortOptions = sortOptions;
        Trend = trend;
        AppId = appId;
    }

    public override IAsyncEnumerator<WorkshopItem> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new SearchEngineAsyncEnumerator(this, Client);
    }

    public string BuildRequestUrl(int page)
    {
        return
            $"https://steamcommunity.com/workshop/browse/?appid={AppId}&browsesort={SortOptions.GetName()}&actualsort={SortOptions.GetName()}&searchtext={SearchText}&section=readytouseitems&created_date_range_filter_start=0&created_date_range_filter_end=0&updated_date_range_filter_start=0&updated_date_range_filter_end=0&p={page}&days={Trend}";
    }

    public class SearchEngineAsyncEnumerator : AbstractFetchEngineAsyncEnumerator<WorkshopItem, SearchEngine>
    {
        public int CurrentPage { get; set; } = 1;

        public SearchEngineAsyncEnumerator(SearchEngine fetchEngine, SteamWorkshopClient client) : base(fetchEngine, client)
        {
        }

        public override async ValueTask<bool> MoveNextAsync()
        {
            switch (await GetResponseAsync(FetchEngine.BuildRequestUrl(CurrentPage)).ConfigureAwait(false))
            {
                case Result<string>.Success(var response):
                    if (!response.Contains("no_items"))
                    {
                        CurrentEntityEnumerator = (await Parser.ParseSearchResult(response).ConfigureAwait(false)).GetEnumerator();
                        CurrentPage++;
                    }
                    else
                    {
                        FetchEngine.EngineHandle.Complete();
                        return false;
                    }

                    break;
                case Result<string>.Failure(var exception):
                    if (exception is { } e)
                    {
                        FetchEngine.EngineHandle.Complete();
                        throw e;
                    }

                    FetchEngine.EngineHandle.Complete();
                    return false;
            }

            if (CurrentEntityEnumerator!.MoveNext())
            {
                return true;
            }

            FetchEngine.EngineHandle.Complete();
            return false;
        }
    }
}