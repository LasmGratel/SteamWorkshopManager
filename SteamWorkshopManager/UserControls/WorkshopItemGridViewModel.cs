using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using SteamWorkshopManager.Client.Engine;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Database;
using SteamWorkshopManager.Model;
using SteamWorkshopManager.Util;
using SuffixTreeSharp;

namespace SteamWorkshopManager.UserControls;

public partial class WorkshopItemGridViewModel : ObservableObject, IDisposable, IWorkshopItemVisualizer
{
    private readonly WorkshopItemVisualizationController _controller;
    
    private Workshop _workshop;

    public Workshop Workshop
    {
        get => _workshop;
        set
        {
            SetProperty(ref _workshop, value);

            if (_workshop != value || Tags.Count == 0)
                Tags = new ObservableCollection<WorkshopItemTagViewModel>(value.Tags.Select(x => new WorkshopItemTagViewModel(x)));
        }
    }

    [ObservableProperty]
    private bool _hasNoItems;

    [ObservableProperty]
    private ObservableCollection<WorkshopItemTagViewModel> _tags;

    private ObservableCollection<WorkshopItemViewModel> _viewModels;

    [ObservableProperty]
    private bool _hasItemSelected;

    [ObservableProperty]
    private SearchEngine.SearchContext _searchContext = new();

    [ObservableProperty]
    private bool _isLoading;

    public GeneralizedSuffixTree SearchTree = new();

    public void DisposeCurrent()
    {
        _controller.FetchEngine?.Cancel();
        _controller.FetchEngine = null;
        foreach (var model in _viewModels)
        {
            model.Dispose();
        }
        SelectedViewModels.Clear();
        ItemsView.Clear();
        _viewModels.Clear();
    }

    public ObservableCollection<WorkshopItemViewModel> ViewModels
    {
        get => _viewModels;
        set
        {
            SetProperty(ref _viewModels, value);
            ItemsView.Source = _viewModels;
            RebuildSearchTree();
        }
    }

    public IncrementalLoadingCollection<FetchEngineIncrementalSource<WorkshopItem, WorkshopItemViewModel>, WorkshopItemViewModel>? IncrementalCollection
    {
        get;
        set;
    }

    public AdvancedCollectionView ItemsView { get; set; }

    public ObservableCollection<WorkshopItemViewModel> SelectedViewModels { get; set; }

    public WorkshopItemGridViewModel()
    {
        _viewModels = new ObservableCollection<WorkshopItemViewModel>();
        SelectedViewModels = new ObservableCollection<WorkshopItemViewModel>();
        SelectedViewModels.CollectionChanged += (sender, args) =>
        {
            HasItemSelected = SelectedViewModels.Count > 0;
        };
        _controller = new WorkshopItemVisualizationController(this);
        ItemsView = new AdvancedCollectionView(ViewModels);
        Tags = new ObservableCollection<WorkshopItemTagViewModel>();
    }

    public void ClearSelection()
    {
        SelectedViewModels.ForEach(x => x.Selected = false);
        SelectedViewModels.Clear();
    }

    private void RebuildSearchTree()
    {
        SearchTree = new GeneralizedSuffixTree();
        for (var i = 0; i < ViewModels.Count; i++)
        {
            SearchTree.Put(ViewModels[i].Item.Name.ToLower(), i);
        }
    }

    public async Task LoadMoreItemsAsync(uint count = 20)
    {
        if (!IsLoading && IncrementalCollection != null)
        {
            IsLoading = true;
            await IncrementalCollection.LoadMoreItemsAsync(count);
            ViewModels.AddRange(IncrementalCollection.Skip(ViewModels.Count));
            IsLoading = false;
        }
    }

    public async Task ResetEngineAndFillAsync(FetchEngine<WorkshopItem?>? newEngine, int? itemLimit = null)
    {
        DisposeCurrent();
        RefreshWorkshop();
        /*var itemAdded = new HashSet<long>();
        _controller.FetchEngine = newEngine!;

        SearchTree = new GeneralizedSuffixTree();
        var i = 0;
        await foreach (var x in _controller.FetchEngine)
        {
            if (_controller.FetchEngine.EngineHandle.IsCompleted)
                break;

            if (x != null && !itemAdded.Contains(x.Id))
            {
                i++;
                SearchTree.Put(x.Name, i);
                itemAdded.Add(x.Id);
                ViewModels.Add(new WorkshopItemViewModel(x));
            }
        }*/
        HasNoItems = !await _controller.ResetAndFillAsync(newEngine, itemLimit);
    }

    public async void RefreshWorkshop()
    {
        Workshop = await AppContext.CacheDatabase.GetApp(Workshop.AppId)!;
    }

    public void Dispose()
    {
        _controller.Dispose();
        GC.SuppressFinalize(this);
    }
}