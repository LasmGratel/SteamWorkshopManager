using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using Microsoft.UI.Xaml.Navigation;
using SteamWorkshopManager.Client.Engine;
using SteamWorkshopManager.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamWorkshopManager.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class HomePage : Page
{
    public ObservableCollection<WorkshopItem> WorkshopItems { get; set; }


    public HomePage()
    {
        this.InitializeComponent();
        WorkshopItems = new ObservableCollection<WorkshopItem>();
    }

    private void HomePage_OnLoaded(object sender, RoutedEventArgs e)
    {

    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ValueTuple<SearchEngine.SearchContext, Workshop> t)
        {
            var (context, workshop) = t;
            WorkshopItemGrid.ViewModel.Workshop = workshop;
            SubscribedItemGrid.ViewModel.Workshop = workshop;
            _ = WorkshopItemGrid.ViewModel.ResetEngineAndFillAsync(new SearchEngine(AppContext.Client, null, context));
            _ = SubscribedItemGrid.ViewModel.ResetEngineAndFillAsync(new SubscribedEngine(AppContext.Client, null, context.AppId));
        }
        else
        {
            WorkshopItemGrid.ViewModel.Workshop = AppContext.CacheDatabase.GetApp(281990).Result!;
            SubscribedItemGrid.ViewModel.Workshop = AppContext.CacheDatabase.GetApp(281990).Result!;
            _ = WorkshopItemGrid.ViewModel.ResetEngineAndFillAsync(new SearchEngine(AppContext.Client, null, 281990));
            _ = SubscribedItemGrid.ViewModel.ResetEngineAndFillAsync(new SubscribedEngine(AppContext.Client, null, 281990));
        }
    }

    public async void Favorite(int appId, long id)
    {
        await AppContext.Client.PostFileFavoriteAsync(appId, id);
    }

    public async void Unfavorite(int appId, long id)
    {
        await AppContext.Client.PostFileFavoriteAsync(appId, id);
    }

    private void CheckBox_Tapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }
}