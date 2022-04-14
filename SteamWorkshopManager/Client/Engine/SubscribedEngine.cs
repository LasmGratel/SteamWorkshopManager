using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Client.Engine;

public class SubscribedEngine : FetchEngine<WorkshopItem>
{
    public long AppId { get; set; }

    public SubscribedEngine(SteamWorkshopClient client, EngineHandle? engineHandle, long appId) : base(client, engineHandle)
    {
        AppId = appId;
    }

    public override IAsyncEnumerator<WorkshopItem> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        return new SubscribedEngineAsyncEnumerator(this, Client);
    }

    public string BuildRequestUrl(int page)
    {
        var url = 
            $"{Client.UserProfileUrl}/myworkshopfiles?browsefilter=mysubscriptions&sortmethod=subscriptiondate&section=items&appid={AppId}&p={page}&numperpage=30&l=english";
        return url;
    }


    public class SubscribedEngineAsyncEnumerator : AbstractFetchEngineAsyncEnumerator<WorkshopItem, SubscribedEngine>
    {
        public int CurrentPage { get; set; } = 1;
        public int MaxPage { get; set; } = 1;

        public SubscribedEngineAsyncEnumerator(SubscribedEngine fetchEngine, SteamWorkshopClient client) : base(fetchEngine, client)
        {
        }

        public override async ValueTask<bool> MoveNextAsync()
        {
            if (FetchEngine.EngineHandle.IsCancelled || FetchEngine.EngineHandle.IsCompleted || CurrentPage > MaxPage)
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
                        if (CurrentPage == 1)
                        {
                            var parser = new HtmlParser();
                            var doc = await parser.ParseDocumentAsync(response);
                            var list = doc.QuerySelectorAll(".pagelink");
                            if (list.Length == 0)
                            {
                                FetchEngine.EngineHandle.Complete();
                                return false;
                            }
                            var pageNum = int.Parse(list.Last().TextContent);
                            MaxPage = pageNum;
                        }

                        var result = await Parser.ParseMySubscriptions(response).ConfigureAwait(false);
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