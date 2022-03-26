using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Model;

public partial class WorkshopItemTagViewModel : ObservableObject
{
    [ObservableProperty]
    private WorkshopItemTag _tag;

    [ObservableProperty]
    private bool _selected;

    public WorkshopItemTagViewModel(WorkshopItemTag tag)
    {
        _tag = tag;
    }
}