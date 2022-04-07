using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using SteamWorkshopManager.Model;

namespace SteamWorkshopManager.Pages;

public partial class CollectionsPageViewModel : ObservableObject
{
    private ObservableCollection<WorkshopCollectionViewModel> _collections = new();

    public ObservableCollection<WorkshopCollectionViewModel> Collections
    {
        get => _collections;
        set
        {
            SetProperty(ref _collections, value);
            CollectionView.Source = value;
        }
    }

    public AdvancedCollectionView CollectionView = new();

    public CollectionsPageViewModel()
    {
        CollectionView.Source = Collections;
    }

    public async Task Load()
    {
        await AppContext.CollectionDatabase.RefreshCollections();
        Collections = new ObservableCollection<WorkshopCollectionViewModel>((await AppContext.CollectionDatabase.GetAllCollections()).Select(x => new WorkshopCollectionViewModel(x)));
    }
}