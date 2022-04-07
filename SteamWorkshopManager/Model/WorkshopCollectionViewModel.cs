using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Model;

public partial class WorkshopCollectionViewModel : ObservableObject
{
    [ObservableProperty]
    private WorkshopCollection _collection;

    public WorkshopCollectionViewModel(WorkshopCollection collection)
    {
        _collection = collection;
    }
}