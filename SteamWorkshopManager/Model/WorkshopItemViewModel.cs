using System;
using System.Threading.Tasks;
using Windows.UI.Text;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SteamWorkshopManager.Attributes;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Util.UI;

namespace SteamWorkshopManager.Model;

public partial class WorkshopItemViewModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    private bool _selected;


    [ObservableProperty]
    private bool _favorited;

    [ObservableProperty]
    private bool _subscribed;

    [ObservableProperty]
    private FontIcon _favoriteIcon;

    public FontIcon GetFavoriteFontIcon(bool favorited)
    {
        return new FontIcon
        {
            Glyph = favorited ? FontIconSymbols.FavoriteStarFillE735.GetMetadataOnEnumMember() : FontIconSymbols.FavoriteStarE734.GetMetadataOnEnumMember(),
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontWeight = new FontWeight(1),
            FontStyle = FontStyle.Normal
        };
    }

    public WorkshopItemViewModel(WorkshopItem item)
    {
        Item = item;
        Favorited = item.Favorited;
        Subscribed = item.Subscribed;
        Selected = false;
        FavoriteIcon = GetFavoriteFontIcon(item.Favorited);
    }

    public WorkshopItem Item { get; }

    public void Dispose()
    {
    }

    public string GetSubscribeButtonContent(bool subscribed)
    {
        return subscribed ? "Unsubscribe" : "Subscribe";
    }

    public Task ToggleFavorite()
    {
        return Favorited ? Unfavorite() : Favorite();
    }

    public Task ToggleSubscribe()
    {
        return Subscribed ? Unsubscribe() : Subscribe();
    }

    public Task Favorite()
    {
        Favorited = true;
        return AppContext.Client.PostFileFavoriteAsync(Item.AppId, Item.Id);
    }

    public Task Unfavorite()
    {
        Favorited = false;
        return AppContext.Client.PostFileUnfavoriteAsync(Item.AppId, Item.Id);
    }

    public Task Subscribe()
    {
        Subscribed = true;
        return AppContext.Client.PostFileSubscribeAsync(Item.AppId, Item.Id);
    }

    public Task Unsubscribe()
    {
        Subscribed = false;
        return AppContext.Client.PostFileUnsubscribeAsync(Item.AppId, Item.Id);
    }
}