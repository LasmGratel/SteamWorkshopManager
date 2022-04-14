using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Model;

public partial class WorkshopItemDetailsViewModel : ObservableObject
{
    private static readonly ReverseMarkdown.Converter markdownConverter = new();

    [ObservableProperty]
    private WorkshopItemDetails? _item;

    [ObservableProperty]
    private string? _descriptionMarkdown;

    public WorkshopItemDetailsViewModel()
    {
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == "Item")
        {
            DescriptionMarkdown = markdownConverter.Convert(Item?.Description ?? "");
        }
    }
}