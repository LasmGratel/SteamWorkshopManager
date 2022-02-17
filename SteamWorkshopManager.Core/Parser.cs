using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Text;

namespace SteamWorkshopManager.Core;

public static class Parser
{
    private const string DateFormat = "d MMM, yyyy @ h:mtt";

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
            var app = item.QuerySelector(".workshopItemApp")?.TextContent;
            var dates = item.QuerySelectorAll(".workshopItemDate");
            var subscribedDate = dates.First().TextContent.ReplaceFirst("Subscribed on ", "");
            var lastUpdatedDate = dates.Last().TextContent.ReplaceFirst("Last Updated ", "");
            var imageUrl = item.QuerySelector(".backgroundImg")?.GetAttribute("src");

            yield return new WorkshopItem
            {
                App = app!,
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

        var description = doc.QuerySelector(".workshopItemDescription")?.TextContent;

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
        
        var revisions =
            long.Parse(
                new string(doc.QuerySelector(".detailsStatNumChangeNotes")?.TextContent.Replace(" Change Notes", "")
                    .Trim().TakeWhile(c => char.IsDigit(c) || c == '.').ToArray()));

        var fileSize = doc.QuerySelector(".detailsStatRight")?.TextContent;

        if (fileSize == null || !fileSize.Contains("MB")) fileSize = "Unknown";

        var requiredItems = doc.QuerySelector("#RequiredItems")?.QuerySelectorAll("a")?.Select(x =>
            long.Parse(x.GetAttribute("href")?.Replace("https://steamcommunity.com/workshop/filedetails/?id=", "") ?? "0")).ToList() ?? new List<long>();

        return new WorkshopItemDetails
        {
            Id = id,
            Authors = authors,
            FileSize = fileSize,
            Visitors = visitors,
            Subscribers = subscribers,
            Favorites = favorites,
            Description = description,
            Revisions = revisions,
            Dependencies = requiredItems,
            Collections = collections,
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