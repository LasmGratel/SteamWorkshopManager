using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamWorkshopManager.Attributes;
using SteamWorkshopManager.Client.Engine;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Pages;
using SteamWorkshopManager.Util;
using SteamWorkshopManager.Util.UI;

namespace SteamWorkshopManager.Model.Navigation;

public record NavigationContext
{
    public static NavigationContext AppsPage = new()
    {
        Tag = "apps",
        Icon = FontIconSymbols.AppsED35,
        Content = "Apps",
        PageType = typeof(AppsPage)
    };

    public static NavigationContext CollectionsPage = new()
    {
        Tag = "collections",
        Icon = FontIconSymbols.ListEA37,
        Content = "Collections",
        PageType = typeof(CollectionsPage)
    };

    public static NavigationContext SettingsPage = new()
    {
        Tag = "settings",
        Icon = FontIconSymbols.SettingE713,
        Content = "Settings",
        PageType = typeof(SettingsPage)
    };

    public static NavigationContext AppPage(Workshop workshop)
    {
        return new NavigationContext
        {
            Tag = $"app-{workshop.AppId}",
            Icon = FontIconSymbols.GameE7FC,
            Content = workshop.Name,
            PageType = typeof(HomePage),
            Parameter = (new SearchEngine.SearchContext {AppId = workshop.AppId}, workshop),
            CanClose = true
        };
    }

    public static NavigationContext CollectionPage(WorkshopCollection collection)
    {
        return new NavigationContext
        {
            Tag = $"collection-{collection.Name}-{collection.Id}",
            Icon = FontIconSymbols.LibraryE8F1,
            Content = collection.Name,
            PageType = typeof(CollectionViewerPage),
            Parameter = collection,
            CanClose = true
        };
    }

    public static NavigationContext ItemPage(long id, string name)
    {
        return new NavigationContext
        {
            Tag = $"item-{id}",
            Icon = FontIconSymbols.LibraryE8F1,
            Content = name,
            PageType = typeof(WorkshopItemPage),
            Parameter = id,
            CanClose = true
        };
    }


    public string Tag { get; set; } = "";

    public FontIconSymbols Icon { get; set; }

    public string IconGlyph => Icon.GetCustomAttribute<Metadata>()?.Key ?? "";

    public string Content { get; set; } = "";

    public Type PageType { get; set; }

    public object? Parameter { get; set; }

    public bool CanClose { get; set; }
}