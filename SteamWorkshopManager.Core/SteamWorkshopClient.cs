using System.Net;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using Newtonsoft.Json;

namespace SteamWorkshopManager.Core;

public class SteamWorkshopClient
{
    private string Cookie { get; }

    private string SessionId { get; }

    private string UserProfileUrl { get; set; }

    private readonly HttpClient _client;

    public SteamWorkshopClient(string? cookie, string? userProfileUrl, HttpClientHandler? handler = null)
    {
        const string sidRegex = "sessionid=(\\w+)";
        var values = Regex.Match(cookie, sidRegex).Groups.Values;
        SessionId = values.Count() > 1 ? Regex.Match(cookie, sidRegex).Groups.Values.Skip(1).First().Value : "";

        Cookie = cookie ?? "";
        UserProfileUrl = userProfileUrl ?? "";

        handler ??= new HttpClientHandler();
        handler.UseCookies = false;
        handler.AutomaticDecompression = DecompressionMethods.GZip;
        _client = new HttpClient(handler);
        _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.7113.93 Safari/537.36");
        _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
        _client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml");
        _client.DefaultRequestHeaders.Add("Accept-Charset", "UTF-8");
        _client.DefaultRequestHeaders.Add("Accept-Language", "en-US");
    }

    public async Task<WorkshopItemDetails> GetWorkshopItemDetailsAsync(long id)
    {
        return await Parser.ParseFileDetails(await _client.GetStringAsync($"https://steamcommunity.com/sharedfiles/filedetails/?id={id}"));
    }

    public async Task<HttpResponseMessage> SendRequest(string url)
    {
        var message = new HttpRequestMessage(HttpMethod.Get, url);

        message.Headers.TryAddWithoutValidation("Cookie", Cookie);
        return await _client.SendAsync(message).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> PostRequest(string url, HttpContent content)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content,
            Version = HttpVersion.Version20,
            VersionPolicy = HttpVersionPolicy.RequestVersionOrLower
        };

        message.Headers.TryAddWithoutValidation("Cookie", Cookie);
        return await _client.SendAsync(message).ConfigureAwait(false);
    }

    public async Task<string> GetBody(string url)
    {
        return await (await SendRequest(url)).Content.ReadAsStringAsync();
    }

    public async Task<List<Workshop>> GetAllWorkshops()
    {
        var count = await GetWorkshopCounts();

        var url = $"https://steamcommunity.com/sharedfiles/ajaxgetworkshops/render/?query=MostRecent&start=0&count={count}";

        var response = await SendRequest(url);
        var body = await response.Content.ReadAsStringAsync();
        dynamic? obj = JsonConvert.DeserializeObject(body);
        if (obj?.success == true)
        {
            return await Parser.ParseWorkshops((string)obj?.results_html);
        }
        else
        {
            throw new Exception("Cannot get workshops from response: ");
        }
    }

    public async Task<int> GetWorkshopCounts()
    {
        const string url = "https://steamcommunity.com/sharedfiles/ajaxgetworkshops/render/?query=MostRecent&start=0&count=0";

        var response = await SendRequest(url);
        var body = await response.Content.ReadAsStringAsync();
        dynamic? obj = JsonConvert.DeserializeObject(body);
        if (obj?.success == true)
        {
            return obj?.total_count;
        }

        throw new Exception("Cannot get workshops from response: ");
    }

    /// <summary>
    /// Search the Steam Workshop
    /// </summary>
    /// <param name="appId">Game id</param>
    /// <param name="searchText">Search criteria</param>
    /// <param name="sortOptions">Sort options in webpage</param>
    /// <param name="trend">If sort options is trend, specify its trend. May be -1(All time), 1, 7, 90, 180, 365</param>
    /// <param name="page">Page num, start from 1</param>
    /// <returns>Workshop item and tag list</returns>
    public async Task<(List<WorkshopItem>, List<WorkshopItemTag>)> SearchWorkshopItems(long appId, string searchText, SortOptions sortOptions, int trend, int page)
    {
        var url =
            $"https://steamcommunity.com/workshop/browse/?appid={appId}&browsesort={sortOptions.GetName()}&actualsort={sortOptions.GetName()}&searchtext={searchText}&section=readytouseitems&created_date_range_filter_start=0&created_date_range_filter_end=0&updated_date_range_filter_start=0&updated_date_range_filter_end=0&p={page}";
        if (sortOptions == SortOptions.Trend)
            url += $"&days={trend}";

        var response = await SendRequest(url);
        var body = await response.Content.ReadAsStringAsync();
        return await Parser.ParseSearchResult(body);
    }

    public async IAsyncEnumerator<WorkshopItem> SearchWorkshopItems(long appId, string searchText = "", SortOptions sortOptions = SortOptions.Trend, int trend = 7)
    {
        var i = 1;
        var url =
            $"https://steamcommunity.com/workshop/browse/?appid={appId}&browsesort={sortOptions.GetName()}&actualsort={sortOptions.GetName()}&searchtext={searchText}&section=readytouseitems&created_date_range_filter_start=0&created_date_range_filter_end=0&updated_date_range_filter_start=0&updated_date_range_filter_end=0&p={i}";
        if (sortOptions == SortOptions.Trend)
            url += $"&days={trend}";

        var response = await SendRequest(url);
        var body = await response.Content.ReadAsStringAsync();
        while (!body.Contains("no_items"))
        {
            foreach (var item in (await Parser.ParseSearchResult(body)).Item1)
            {
                yield return item;
            }

            i++;
            url =
                $"https://steamcommunity.com/workshop/browse/?appid={appId}&browsesort={sortOptions.GetName()}&actualsort={sortOptions.GetName()}&searchtext={searchText}&section=readytouseitems&created_date_range_filter_start=0&created_date_range_filter_end=0&updated_date_range_filter_start=0&updated_date_range_filter_end=0&p={i}";
            response = await SendRequest(url);
            body = await response.Content.ReadAsStringAsync();
        }

    }

    public async IAsyncEnumerable<WorkshopItem> GetAllSubscribedItems(int appId)
    {
        var url =
            $"{UserProfileUrl}/myworkshopfiles?browsefilter=mysubscriptions&sortmethod=subscriptiondate&section=items&appid={appId}&p=1&numperpage=30";

        var response = await SendRequest(url);
        var body = await response.Content.ReadAsStringAsync();

        var parser = new HtmlParser();
        var doc = await parser.ParseDocumentAsync(body);
        var pageNum = long.Parse(doc.QuerySelectorAll(".pagelink").Last().TextContent);

        await foreach (var item in Parser.ParseMySubscriptions(body))
            yield return item;

        if (pageNum > 1)
        {
            for (var i = 2; i <= pageNum; i++)
            {
                await foreach (var item in GetSubscribedItemsAsync(appId, i, 30))
                    yield return item;
            }
        }
    }

    public async Task VoteUp(long id)
    {
        (await PostRequest("https://steamcommunity.com/sharedfiles/voteup",
            new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("id", $"{id}"),
                new("sessionid", SessionId)
            }))).EnsureSuccessStatusCode();
    }

    public async Task VoteDown(long id)
    {
        (await PostRequest("https://steamcommunity.com/sharedfiles/votedown",
            new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("id", $"{id}"),
                new("sessionid", SessionId)
            }))).EnsureSuccessStatusCode();
    }

    public async Task PostSubscribeCollectionAsync(int appId, long id)
    {
        (await PostRequest("https://steamcommunity.com/sharedfiles/subscribecollection",
            new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("id", $"{id}"),
                new("appid", $"{appId}"),
                new("sessionid", SessionId)
            }))).EnsureSuccessStatusCode();
    }

    public async Task PostUnsubscribeCollectionAsync(long id)
    {
        (await PostRequest("https://steamcommunity.com/sharedfiles/unsubscribecollection",
            new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("publishedfileid", $"{id}"),
                new KeyValuePair<string, string>("sessionid", SessionId)
            }))).EnsureSuccessStatusCode();
    }

    public async Task PostFileSubscribeAsync(long appId, long id)
    {
        (await PostRequest("https://steamcommunity.com/sharedfiles/subscribe",
            new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("id", $"{id}"),
                new("appid", $"{appId}"),
                new("sessionid", SessionId)
            }))).EnsureSuccessStatusCode();
    }

    public async Task PostFileUnsubscribeAsync(long appId, long id)
    {
        (await PostRequest("https://steamcommunity.com/sharedfiles/unsubscribe",
            new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("id", $"{id}"),
                new("appid", $"{appId}"),
                new("sessionid", SessionId)
            }))).EnsureSuccessStatusCode();
    }

    public async Task PostFileFavoriteAsync(long appId, long id)
    {
        (await PostRequest("https://steamcommunity.com/sharedfiles/favorite",
            new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("id", $"{id}"),
                new("appid", $"{appId}"),
                new("sessionid", SessionId)
            })).ConfigureAwait(false)).EnsureSuccessStatusCode();
    }

    public async Task PostFileUnfavoriteAsync(long appId, long id)
    {
        (await PostRequest("https://steamcommunity.com/sharedfiles/unfavorite",
            new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("id", $"{id}"),
                new("appid", $"{appId}"),
                new("sessionid", SessionId)
            })).ConfigureAwait(false)).EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Get myworkshopfiles?browsefilter=mysubscriptions items
    /// </summary>
    /// <param name="appId">AppId of a game</param>
    /// <param name="page">Page starts from 1</param>
    /// <param name="pageSize">Must be one of 10, 20, 30.</param>
    /// <returns></returns>
    public async IAsyncEnumerable<WorkshopItem> GetSubscribedItemsAsync(long appId, int page = 1, int pageSize = 10)
    {
        var url =
            $"{UserProfileUrl}/myworkshopfiles?browsefilter=mysubscriptions&sortmethod=subscriptiondate&section=items&appid={appId}&p={page}&numperpage={pageSize}";

        var response = await SendRequest(url);
        var body = await response.Content.ReadAsStringAsync();

        await foreach (var item in Parser.ParseMySubscriptions(body))
            yield return item;
    }

    public async Task AddToCollectionAsync(long collectionId, long itemId)
    {
        (await PostRequest("https://steamcommunity.com/sharedfiles/addchild",
            new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("id", $"{collectionId}"),
                new("childid", $"{itemId}"),
                new("sessionid", SessionId)
            }))).EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Remove a item from your collection
    /// </summary>
    /// <param name="collectionId">Collection id</param>
    /// <param name="itemId">Item id</param>
    /// <returns>Success code, 1 = success</returns>
    public async Task<int> RemoveFromCollectionAsync(long collectionId, long itemId)
    {
        var response = (await PostRequest("https://steamcommunity.com/sharedfiles/removechild",
            new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("id", $"{collectionId}"),
                new("childid", $"{itemId}"),
                new("sessionid", SessionId),
                new("ajax", "true")
            }))).EnsureSuccessStatusCode();
        dynamic? data = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync() ?? "{}");
        return data?.success ?? 0;
    }

    public async IAsyncEnumerable<WorkshopCollection> GetMyCollectionsAsync(int appId)
    {
        var response = (await PostRequest("https://steamcommunity.com/sharedfiles/ajaxgetmycollections",
            new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("appid", $"{appId}"),
                new("sessionid", SessionId)
            }))).EnsureSuccessStatusCode();
        dynamic data = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync() ?? "{}")!;
        foreach (var item in data.all_collections.publishedfiledetails)
        {
            var collection = new WorkshopCollection
            {
                AppId = appId,
                Name = item.title,
                ShortDescription = item.short_description,
                ImageUrl = item.file_url,
                Favorites = item.favorited,
                Subscribers = item.subscriptions,
                Visitors = item.views,
                Id = item.publishedfileid,
                CreatedDate = DateTimeOffset.FromUnixTimeMilliseconds(item.time_created),
                LastUpdatedDate = DateTimeOffset.FromUnixTimeMilliseconds(item.time_updated),
            };
            foreach (var tag in item.tags)
            {
                collection.Tags.Add(tag.tag);
            }
            yield return collection;
        }
    }
}