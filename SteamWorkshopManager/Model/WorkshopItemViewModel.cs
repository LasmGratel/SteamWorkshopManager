using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Model;

public class WorkshopItemViewModel : ObservableObject, IDisposable
{
    public WorkshopItemViewModel()
    {
    }

    public WorkshopItemViewModel(WorkshopItem item)
    {
        Item = item;
    }

    public WorkshopItem Item { get; }

    public void Dispose()
    {
    }
}