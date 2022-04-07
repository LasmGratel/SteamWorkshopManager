using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Model;

public partial class WorkshopItemDetailsViewModel : ObservableObject
{
    private static ReverseMarkdown.Converter markdownConverter = new();

    [ObservableProperty]
    private WorkshopItemDetails _item;

    public WorkshopItemDetailsViewModel(WorkshopItemDetails item)
    {
        Item = item;
    }

    public string DescriptionMarkdown => markdownConverter.Convert(Item.Description);
}