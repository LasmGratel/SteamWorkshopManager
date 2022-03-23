using Windows.Storage;
using Windows.UI.Core;
using LiteDB;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Database;
using SteamWorkshopManager.Util;

namespace SteamWorkshopManager;

public static partial class AppContext
{
    private const string SessionContainerKey = "Session";
    public static readonly ApplicationDataContainer SessionContainer;
    public static MainWindow MainWindow;

    public static SteamWorkshopClient Client { get; set; }
    public static WorkshopItemDatabase ItemDatabase => new(App.Instance.AppServicesScope.ServiceProvider.GetService(typeof(LiteDatabase)) as LiteDatabase, Client);
    public static CacheDatabase CacheDatabase => new(App.Instance.AppServicesScope.ServiceProvider.GetService(typeof(LiteDatabase)) as LiteDatabase, Client);

    public static CoreDispatcher Dispatcher => MainWindow.CoreWindow.Dispatcher;

    public static readonly string DatabaseFilePath = AppKnownFolders.Local.Resolve("SteamWorkshopManager.litedb");


    static AppContext()
    {
        if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(SessionContainerKey))
        {
            ApplicationData.Current.LocalSettings.CreateContainer(SessionContainerKey, ApplicationDataCreateDisposition.Always);
        }

        SessionContainer = ApplicationData.Current.LocalSettings.Containers[SessionContainerKey];
    }
}