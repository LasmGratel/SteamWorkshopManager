using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;

namespace SteamWorkshopManager.Core;

public class SteamWorkshopClient
{
    private string Cookie { get; }

    private string UserId { get; }

    private string SessionId { get; }

    private string UserProfileUrl { get; set; }

    private readonly HttpClient _client;

    public SteamWorkshopClient(string cookie, string userProfileUrl, HttpClientHandler? handler = null)
    {
        const string sidRegex = "sessionid=(\\w+)";
        var values = Regex.Match(cookie, sidRegex).Groups.Values;
        SessionId = values.Count() > 1 ? Regex.Match(cookie, sidRegex).Groups.Values.Skip(1).First().Value : "";

        Cookie = cookie;
        UserProfileUrl = userProfileUrl;

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

    public async Task<HttpResponseMessage> SendRequest(string url)
    {
        var message = new HttpRequestMessage(HttpMethod.Get, url);

        message.Headers.TryAddWithoutValidation("Cookie", Cookie);
        return await _client.SendAsync(message);
    }

    public async Task<string> GetBody(string url)
    {
        return await (await SendRequest(url)).Content.ReadAsStringAsync();
    }

    public async IAsyncEnumerable<WorkshopItem> GetAllSubscribedItems(int appId)
    {
        var url =
            $"{UserProfileUrl}/myworkshopfiles?browsefilter=mysubscriptions&sortmethod=subscriptiondate&section=items&appid={appId}&p=1&numperpage=30";

        var response = await SendRequest(url);
        var body = await response.Content.ReadAsStringAsync();

        var parser = new HtmlParser();
        var doc = await parser.ParseDocumentAsync(body);
        Console.WriteLine(body);
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

    public async Task PostSubscribeCollectionAsync(int appId, long id)
    {
        (await _client.PostAsync("https://steamcommunity.com/sharedfiles/subscribecollection",
            new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("id", $"{id}"),
                new("appid", $"{appId}"),
                new("sessionid", SessionId)
            }))).EnsureSuccessStatusCode();
    }

    public async Task PostUnsubscribeCollectionAsync(long id)
    {
        (await _client.PostAsync("https://steamcommunity.com/sharedfiles/unsubscribecollection",
            new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("publishedfileid", $"{id}"),
                new KeyValuePair<string, string>("sessionid", SessionId)
            }))).EnsureSuccessStatusCode();
    }

    public async Task PostFileSubscribeAsync(int appId, long id)
    {
        (await _client.PostAsync("https://steamcommunity.com/sharedfiles/subscribe",
            new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("id", $"{id}"),
                new("appid", $"{appId}"),
                new("sessionid", SessionId)
            }))).EnsureSuccessStatusCode();
    }

    public async Task PostFileUnsubscribeAsync(int appId, long id)
    {
        (await _client.PostAsync("https://steamcommunity.com/sharedfiles/unsubscribe",
            new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("id", $"{id}"),
                new("appid", $"{appId}"),
                new("sessionid", SessionId)
            }))).EnsureSuccessStatusCode();
    }

    public async Task PostFileFavoriteAsync(int appId, long id)
    {
        (await _client.PostAsync("https://steamcommunity.com/sharedfiles/favorite",
            new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("id", $"{id}"),
                new("appid", $"{appId}"),
                new("sessionid", SessionId)
            }))).EnsureSuccessStatusCode();
    }

    public async Task PostFileUnfavoriteAsync(int appId, long id)
    {
        (await _client.PostAsync("https://steamcommunity.com/sharedfiles/unfavorite",
            new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("id", $"{id}"),
                new("appid", $"{appId}"),
                new("sessionid", SessionId)
            }))).EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Get myworkshopfiles?browsefilter=mysubscriptions items
    /// </summary>
    /// <param name="appId">AppId of a game</param>
    /// <param name="page">Page starts from 1</param>
    /// <param name="pageSize">Must be one of 10, 20, 30.</param>
    /// <returns></returns>
    public async IAsyncEnumerable<WorkshopItem> GetSubscribedItemsAsync(int appId, int page = 1, int pageSize = 10)
    {
        var url =
            $"{UserProfileUrl}/myworkshopfiles?browsefilter=mysubscriptions&sortmethod=subscriptiondate&section=items&appid={appId}&p={page}&numperpage={pageSize}";

        var response = await SendRequest(url);
        var body = await response.Content.ReadAsStringAsync();

        await foreach (var item in Parser.ParseMySubscriptions(body))
            yield return item;
    }
}