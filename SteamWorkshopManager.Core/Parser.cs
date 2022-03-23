using System.Globalization;
using AngleSharp;
using AngleSharp.Html.Parser;
using AngleSharp.Text;
using AngleSharp.Xhtml;
using Newtonsoft.Json;

namespace SteamWorkshopManager.Core;

public static class Parser
{
    private const string DateFormat = "d MMM, yyyy @ h:mtt";

    public static async Task<List<Workshop>> ParseWorkshops(string source)
    {
        var parser = new HtmlParser();
        var doc = await parser.ParseDocumentAsync(source);

        return doc.QuerySelectorAll(".app")
            .Select(element =>
            {
                var appId = element.GetAttribute("onClick")?.Split('/').GetValue(4) as string ??
                       throw new FormatException("Cannot parse " + element);
                var name = element.QuerySelector(".appLogo")?.GetAttribute("alt") ?? "";
                return new Workshop { AppId = long.Parse(appId), Name = name };
            })
            .ToList();
    }

    public static async Task<List<WorkshopItem>> ParseSearchResult(string source)
    {
        var parser = new HtmlParser();
        var doc = await parser.ParseDocumentAsync(source);

        var list = new List<WorkshopItem>();
        var items = doc.QuerySelector(".workshopBrowseItems")!;
        var scripts = items.QuerySelectorAll("script").ToList();
        var elements = items.QuerySelectorAll(".workshopItem").ToList();
        for (var i = 0; i < elements.Count; i++)
        {
            var script = scripts[i].TextContent;
            dynamic obj = JsonConvert.DeserializeObject(script[script.IndexOf('{')..(script.LastIndexOf('}') + 1)])!;
            var element = elements[i];
            var imageUrl = element.QuerySelector(".workshopItemPreviewImage")?.GetAttribute("src")?.AsSpan()
                .SubstringBeforeLast('/') + '/';

            list.Add(new WorkshopItem
            {
                Id = obj.id,
                Name = obj.title,
                AppId = obj.appid,
                ImageUrl = imageUrl
            });
        }

        return list;
    }

    /// <summary>
    /// Parse steamcommunity/myworkshopfiles?browsefilter=mysubscriptions
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static async IAsyncEnumerable<WorkshopItem> ParseMySubscriptions(string source)
    {
        var parser = new HtmlParser();
        var doc = await parser.ParseDocumentAsync(source);

        var items = doc.GetElementsByClassName("workshopItemSubscription").Where(x => x.Id != null && x.Id.StartsWith("Subscription"));
        foreach (var item in items)
        {
            var id = item.Id!.Replace("Subscription", "");
            var title = item.QuerySelector(".workshopItemTitle")?.TextContent;
            var app = item.QuerySelector($"#favorite_{id}")?.GetAttribute("onclick")?.Split(" ")[1]!;
            var dates = item.QuerySelectorAll(".workshopItemDate");
            var subscribedDate = dates.First().TextContent.ReplaceFirst("Subscribed on ", "");
            var lastUpdatedDate = dates.Last().TextContent.ReplaceFirst("Last Updated ", "");
            var imageUrl = item.QuerySelector(".backgroundImg")?.GetAttribute("src");

            yield return new WorkshopItem
            {
                AppId = long.Parse(app),
                Id = long.Parse(id),
                Name = title!,
                ImageUrl = imageUrl!,
                SubscribedDate = ParseSteamDateTime(subscribedDate),
                LastUpdatedDate = ParseSteamDateTime(lastUpdatedDate)
            };
        }
    }

    public static WorkshopItem ParseWorkshopItem()
    {
        return new WorkshopItem();
    }

    public static async Task<WorkshopItemDetails> ParseFileDetails(string source)
    {
        var parser = new HtmlParser();
        var doc = await parser.ParseDocumentAsync(source);

        var id = long.Parse(doc.QuerySelector(".sectionTab")?.GetAttribute("href")
            ?.Replace("https://steamcommunity.com/sharedfiles/filedetails/?id=", "") ?? "0");

        var description = doc.QuerySelector(".workshopItemDescription")?.ToHtml(XhtmlMarkupFormatter.Instance);

        // author profile
        var authors = (from element in doc.QuerySelectorAll(".friendBlock")
            let url = element.QuerySelector(".friendBlockLinkOverlay")?.GetAttribute("href") ?? ""
            let name = element.QuerySelector(".friendBlockContent")?.TextContent
            let avatar = element.QuerySelector(".playerAvatar")?.QuerySelector("img")?.GetAttribute("src") ?? ""
            select new SteamProfile { Url = url, Name = name, Avatar = avatar }
            ).ToList();

        var collections = long.Parse(doc.QuerySelector(".parentCollectionsNumOthers")?.QuerySelector("a")?.TextContent.Replace(" collections", "") ?? "0");

        var stats = doc.QuerySelectorAll(".stats_table td").ToList();
        var visitors = long.Parse(stats[0].TextContent, NumberStyles.AllowThousands);
        var subscribers = long.Parse(stats[2].TextContent, NumberStyles.AllowThousands);
        var favorites = long.Parse(stats[4].TextContent, NumberStyles.AllowThousands);

        var starsStr = doc.QuerySelector(".fileRatingDetails")?.QuerySelector("img")?.GetAttribute("src") ?? "https://community.cloudflare.steamstatic.com/public/images/sharedfiles/not-yet_large.png?v=2";
        var stars = 0;

        if (starsStr != "https://community.cloudflare.steamstatic.com/public/images/sharedfiles/not-yet_large.png?v=2")
        {
            stars = int.Parse(new string(starsStr.AsSpan().SubstringAfterLast('/').TakeWhile(char.IsDigit).ToArray()));
        }

        var revisions =
            long.Parse(
                new string(doc.QuerySelector(".detailsStatNumChangeNotes")?.TextContent.Replace(" Change Notes", "")
                    .Trim().TakeWhile(char.IsDigit).ToArray()));

        var fileSize = doc.QuerySelector(".detailsStatRight")?.TextContent;

        if (fileSize == null || !fileSize.Contains("MB")) fileSize = "Unknown";

        var requiredItems = doc.QuerySelector("#RequiredItems")?.QuerySelectorAll("a")?.Select(x =>
            long.Parse(x.GetAttribute("href")?.Replace("https://steamcommunity.com/workshop/filedetails/?id=", "") ?? "0")).ToList() ?? new List<long>();

        var app = doc.QuerySelector(".breadcrumbs")?.QuerySelector("a")?.GetAttribute("href")?[31..] ?? "";
        var title = doc.QuerySelector(".workshopItemTitle")?.TextContent ?? "";
        var imgUrl = doc.QuerySelector("#previewImageMain")?.GetAttribute("src")?.AsSpan().SubstringBeforeLast('/') ?? "";
        var lastUpdatedDate = doc.QuerySelectorAll(".detailsStatRight").Last().TextContent;

        return new WorkshopItemDetails
        {
            Id = id,
            Name = title,
            AppId = long.Parse(app),
            ImageUrl = imgUrl + "/",
            SubscribedDate = null,
            LastUpdatedDate = ParseSteamDateTime(lastUpdatedDate),
            Authors = authors,
            FileSize = fileSize,
            Visitors = visitors,
            Subscribers = subscribers,
            Favorites = favorites,
            Description = description,
            Revisions = revisions,
            Dependencies = requiredItems,
            Collections = collections,
            Stars = stars,
        };
    }

    public static DateTime ParseSteamDateTime(string s)
    {
        try
        {
            return DateTime.ParseExact(s, DateFormat, new CultureInfo("en-US"));
        }
        catch (FormatException)
        {
            return DateTime.ParseExact(s.Replace(" @", $", {DateTime.Now.Year} @"), DateFormat, new CultureInfo("en-US"));
        }
    }
}