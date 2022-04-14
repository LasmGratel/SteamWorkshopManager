using System.Net;
using System.Net.Http;
using Windows.Storage;
using Windows.UI.Core;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using SteamWorkshopManager.Client.Service;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Database;
using SteamWorkshopManager.Model.Navigation;
using SteamWorkshopManager.Util;

namespace SteamWorkshopManager;

public static partial class AppContext
{
    public const string CERTIFICATE_TAG = "#SteamWorkshopManager";
    private const string SessionContainerKey = "Session";
    public static readonly ApplicationDataContainer SessionContainer;
    public static MainWindow MainWindow;

    public static NavigationController MainNavigationController;

    public static SteamWorkshopClient Client => new SteamWorkshopClient(
        Settings.Cookie,
        Settings.UserLink,
        new HttpClientHandler
        {//
            Proxy = new WebProxy(Settings.Proxy),
            UseCookies = false
        });

    public static AppSettings Settings => new(SessionContainer);

    public static WorkshopItemDatabase ItemDatabase => new(App.Instance.AppServicesScope.ServiceProvider.GetService(typeof(LiteDatabase)) as LiteDatabase, Client);
    public static CacheDatabase CacheDatabase => new(App.Instance.AppServicesScope.ServiceProvider.GetService(typeof(LiteDatabase)) as LiteDatabase, Client);
    public static CollectionDatabase CollectionDatabase => new(App.Instance.AppServicesScope.ServiceProvider.GetService(typeof(LiteDatabase)) as LiteDatabase, Client);

    public static CoreDispatcher Dispatcher => MainWindow.CoreWindow.Dispatcher;

    public static readonly string DatabaseFilePath = AppKnownFolders.Local.Resolve("SteamWorkshopManager.litedb");

    public static string ResolveLocalFilePath(string path)
    {
        return AppKnownFolders.Local.Resolve(path);
    }

    static AppContext()
    {
        if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(SessionContainerKey))
        {
            ApplicationData.Current.LocalSettings.CreateContainer(SessionContainerKey, ApplicationDataCreateDisposition.Always);
        }

        SessionContainer = ApplicationData.Current.LocalSettings.Containers[SessionContainerKey];
        _ = Settings; // Create instance now
    }
}