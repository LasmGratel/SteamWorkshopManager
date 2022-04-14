using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Util;
using SuffixTreeSharp;

namespace SteamWorkshopManager.Pages;

public partial class CreateNewCollectionDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _errorInfo;

    [ObservableProperty]
    private bool _showError;

    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private Workshop? _app;

    [ObservableProperty]
    private bool _canEditApp;


    public ObservableCollection<Workshop> AppsCollection = new();

    public ObservableCollection<Workshop> FilteredApps { get; set; } = new();

    private GeneralizedSuffixTree _appsSearchTree = new();

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == "CanEditApp" && CanEditApp)
        {
            AppsCollection.AddRange(AppContext.CacheDatabase.GetApps());
            FilteredApps.AddRange(AppsCollection.Take(10));

            for (var i = 0; i < AppsCollection.Count; i++)
            {
                _appsSearchTree.Put(AppsCollection[i].Name.ToLower(), i);
            }
        }
    }

    public void ResetFilter(string text)
    {
        FilteredApps.Clear();
        if (text.Length > 0)
            FilteredApps.AddRange(_appsSearchTree.Search(text.ToLower()).Take(10).Select(x => AppsCollection[x]));
        else
            FilteredApps.AddRange(AppsCollection.Take(10));
    }
}