using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
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
        AppContext.Client = new SteamWorkshopClient(
            AppContext.SessionContainer.Values["Cookie"].ToString(),
            AppContext.SessionContainer.Values["UserLink"].ToString(),
            new HttpClientHandler
            {
                Proxy = new WebProxy("http://127.0.0.1:1081"),
                UseCookies = false
            });

        _ = WorkshopItemGrid.ViewModel.ResetEngineAndFillAsync(new SearchEngine(AppContext.Client));
    }

    private void RefreshLogin_OnClick(object sender, RoutedEventArgs e)
    {
        AppContext.MainWindow.RootFrame.Navigate(typeof(LoginPage), AppContext.SessionContainer.Values["Cookie"]?.ToString());

    }

    private void LogoutButton_OnClick(object sender, RoutedEventArgs e)
    {
        AppContext.SessionContainer.Values.Remove("Cookie");
        AppContext.SessionContainer.Values.Remove("UserLink");
        AppContext.MainWindow.RootFrame.Navigate(typeof(LoginPage), "");
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