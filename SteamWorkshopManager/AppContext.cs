using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.UI.Xaml;

namespace SteamWorkshopManager;

public static partial class AppContext
{
    private const string SessionContainerKey = "Session";
    public static readonly ApplicationDataContainer SessionContainer;
    public static MainWindow MainWindow;

    static AppContext()
    {
        if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(SessionContainerKey))
        {
            ApplicationData.Current.LocalSettings.CreateContainer(SessionContainerKey, ApplicationDataCreateDisposition.Always);
        }

        SessionContainer = ApplicationData.Current.LocalSettings.Containers[SessionContainerKey];
    }
}