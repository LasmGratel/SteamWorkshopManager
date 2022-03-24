using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using SteamWorkshopManager.Client.Engine;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Model;

namespace SteamWorkshopManager.UserControls;

public class WorkshopItemVisualizationController
{
    private readonly IWorkshopItemVisualizer _visualizer;

    public FetchEngine<WorkshopItem?>? FetchEngine { get; set; }

    public NotifyCollectionChangedEventHandler? CollectionChanged { get; set; }

    public WorkshopItemVisualizationController(IWorkshopItemVisualizer visualizer)
    {
        _visualizer = visualizer;
    }

    public async Task<bool> FillAsync(int? itemsLimit = null)
    {
        var collection = new IncrementalLoadingCollection<FetchEngineIncrementalSource<WorkshopItem, WorkshopItemViewModel>, WorkshopItemViewModel>(new WorkshopItemFetchEngineIncrementalSource(FetchEngine!, itemsLimit));
        _visualizer.ViewModels = collection;
        await collection.LoadMoreItemsAsync(20);
        _visualizer.ViewModels.CollectionChanged += CollectionChanged;
        return _visualizer.ViewModels.Count > 0;
    }

    public async Task FillAsync(FetchEngine<WorkshopItem?>? newEngine, int? itemsLimit = null)
    {
        FetchEngine = newEngine;
        await FillAsync(itemsLimit);
    }

    public Task<bool> ResetAndFillAsync(FetchEngine<WorkshopItem?>? newEngine, int? itemLimit = null)
    {
        FetchEngine?.EngineHandle.Cancel();
        FetchEngine = newEngine; 
        _visualizer.DisposeCurrent();
        return FillAsync(itemLimit);
    }

    public void Dispose()
    {
        _visualizer.DisposeCurrent();
        FetchEngine = null;
    }
}