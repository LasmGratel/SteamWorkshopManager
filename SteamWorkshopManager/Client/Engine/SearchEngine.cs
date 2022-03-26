using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Util;

namespace SteamWorkshopManager.Client.Engine;

public class SearchEngine : FetchEngine<WorkshopItem>
{
    public SearchContext Context { get; set; }

    public SearchEngine(SteamWorkshopClient client, EngineHandle? engineHandle, long appId, string searchText = "", SortOptions sortOptions = SortOptions.Trend, int trend = 7, IEnumerable<string>? tags = null) : base(client, engineHandle)
    {
        Context = new SearchContext
        {
            SearchText = searchText,
            SortOptions = sortOptions,
            Trend = trend,
            AppId = appId,
            Tags = tags ?? Array.Empty<string>(),
        };
    }

    public SearchEngine(SteamWorkshopClient client, EngineHandle? engineHandle, SearchContext context) : base(client, engineHandle)
    {
        Context = context;
    }

    public override IAsyncEnumerator<WorkshopItem> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new SearchEngineAsyncEnumerator(this, Client);
    }

    public string BuildRequestUrl(int page)
    {
        var url =
            $"https://steamcommunity.com/workshop/browse/?appid={Context.AppId}&browsesort={Context.SortOptions.GetName()}&actualsort={Context.SortOptions.GetName()}&searchtext={Context.SearchText}&section=readytouseitems&created_date_range_filter_start=0&created_date_range_filter_end=0&updated_date_range_filter_start=0&updated_date_range_filter_end=0&p={page}&days={Context.Trend}";
        Context.Tags.ForEach(tag => url += $"&requiredtags[]={tag}");
        return url;
    }

    public record SearchContext
    {
        public long AppId { get; set; }
        public string SearchText { get; set; } = "";
        public SortOptions SortOptions { get; set; } = SortOptions.Trend;
        public int Trend { get; set; } = 7;
        public IEnumerable<string> Tags { get; set; } = Array.Empty<string>();
    }

    public class SearchEngineAsyncEnumerator : AbstractFetchEngineAsyncEnumerator<WorkshopItem, SearchEngine>
    {
        public int CurrentPage { get; set; } = 1;

        public SearchEngineAsyncEnumerator(SearchEngine fetchEngine, SteamWorkshopClient client) : base(fetchEngine, client)
        {
        }

        public override async ValueTask<bool> MoveNextAsync()
        {
            if (FetchEngine.EngineHandle.IsCancelled || FetchEngine.EngineHandle.IsCompleted)
            {
                FetchEngine.EngineHandle.Complete();
                return false;
            }

            switch (CurrentEntityEnumerator?.MoveNext())
            {
                case true:
                    return true;
                case false: // No more items on this page
                    CurrentEntityEnumerator = null;
                    CurrentPage++;
                    break;
            }

            switch (await GetResponseAsync(FetchEngine.BuildRequestUrl(CurrentPage)).ConfigureAwait(false))
            {
                case Result<string>.Success(var response):
                    if (!response.Contains("no_items") && !FetchEngine.EngineHandle.IsCancelled)
                    {
                        var (result, tags) = await Parser.ParseSearchResult(response).ConfigureAwait(false);
                        AppContext.CacheDatabase.UpdateTags(FetchEngine.Context.AppId, tags);
                        CurrentEntityEnumerator = result.GetEnumerator();
                        return true;
                    }
                    else
                    {
                        FetchEngine.EngineHandle.Complete();
                        return false;
                    }

                case Result<string>.Failure(var exception):
                    if (exception is { } e)
                    {
                        FetchEngine.EngineHandle.Complete();
                        throw e;
                    }

                    FetchEngine.EngineHandle.Complete();
                    return false;
            }

            FetchEngine.EngineHandle.Complete();
            return false;
        }
    }
}