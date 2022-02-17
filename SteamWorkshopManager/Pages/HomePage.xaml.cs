using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Microsoft.UI.Xaml.Media.Animation;
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
    public SteamWorkshopClient Client { get; set; }

    public HomePage()
    {
        this.InitializeComponent();
        WorkshopItems = new ObservableCollection<WorkshopItem>();
    }

    private void HomePage_OnLoaded(object sender, RoutedEventArgs e)
    {
        TextBlock.Text = AppContext.SessionContainer.Values["Cookie"].ToString() + "\n" + AppContext.SessionContainer.Values["UserLink"].ToString();
        Client = new SteamWorkshopClient(AppContext.SessionContainer.Values["Cookie"].ToString(),
            AppContext.SessionContainer.Values["UserLink"].ToString(),
            new HttpClientHandler
            {
                Proxy = new WebProxy("http://127.0.0.1:1081"),
                UseCookies = false
            });
    }

    private void RefreshLogin_OnClick(object sender, RoutedEventArgs e)
    {
        AppContext.MainWindow.RootFrame.Navigate(typeof(LoginPage), AppContext.SessionContainer.Values["Cookie"]?.ToString());

    }

    private async void RefreshItems_OnClick(object sender, RoutedEventArgs e)
    {
        Client = new SteamWorkshopClient(
            AppContext.SessionContainer.Values["Cookie"].ToString(),
            AppContext.SessionContainer.Values["UserLink"].ToString(),
            new HttpClientHandler
            {
                Proxy = new WebProxy("http://127.0.0.1:1081"),
                UseCookies = false
            });
        WorkshopItems.Clear();
        await foreach (var item in Client.GetSubscribedItemsAsync(255710))
        {
            WorkshopItems.Add(item);
        }
    }

    private void LogoutButton_OnClick(object sender, RoutedEventArgs e)
    {
        AppContext.SessionContainer.Values.Remove("Cookie");
        AppContext.SessionContainer.Values.Remove("UserLink");
        AppContext.MainWindow.RootFrame.Navigate(typeof(LoginPage), "");
    }
}