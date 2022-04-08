using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Util;

namespace SteamWorkshopManager.Client.Engine;

public partial class SearchEngine : FetchEngine<WorkshopItem>
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
            Tags = new ObservableCollection<string>(tags ?? Array.Empty<string>()),
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

    public partial class SearchContext : ObservableObject
    {
        [ObservableProperty]
        private long _appId;

        [ObservableProperty]
        private string _searchText = "";

        [ObservableProperty]
        private SortOptions _sortOptions = SortOptions.Trend;

        [ObservableProperty]
        private int _trend = 7;

        [ObservableProperty]
        private ObservableCollection<string> _tags = new();

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

            switch (await GetResponseAsync(FetchEngine.BuildRequestUrl(CurrentPage)))
            {
                case Result<string>.Success(var response):
                    if (!response.Contains("no_items") && !FetchEngine.EngineHandle.IsCancelled)
                    {
                        var (result, tags) = await Parser.ParseSearchResult(response);
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