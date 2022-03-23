using System.Collections.ObjectModel;
using SteamWorkshopManager.Model;

namespace SteamWorkshopManager.UserControls;

public interface IWorkshopItemVisualizer
{
    /// <summary>
    /// Dispose current visualizing illustrations, behaves like Clear
    /// </summary>
    void DisposeCurrent();

    ObservableCollection<WorkshopItemViewModel> ViewModels { get; set; }
}