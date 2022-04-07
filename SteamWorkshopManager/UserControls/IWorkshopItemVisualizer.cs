using System.Collections.ObjectModel;
using SteamWorkshopManager.Client.Engine;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Model;
using SteamWorkshopManager.Util;

namespace SteamWorkshopManager.UserControls;

public interface IWorkshopItemVisualizer
{
    /// <summary>
    /// Dispose current visualizing illustrations, behaves like Clear
    /// </summary>
    void DisposeCurrent();

    ObservableCollection<WorkshopItemViewModel> ViewModels { get; set; }

    IncrementalLoadingCollection<FetchEngineIncrementalSource<WorkshopItem, WorkshopItemViewModel>, WorkshopItemViewModel> IncrementalCollection { get; set; }
}