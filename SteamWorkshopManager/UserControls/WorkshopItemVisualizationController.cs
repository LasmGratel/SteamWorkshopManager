using System;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using SteamWorkshopManager.Client.Engine;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Model;
using SteamWorkshopManager.Util;

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

    public async ValueTask<bool> FillAsync(int? itemsLimit = null)
    {
        if (FetchEngine == null) return false;
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

    public ValueTask<bool> ResetAndFillAsync(FetchEngine<WorkshopItem?>? newEngine, int? itemLimit = null)
    {
        FetchEngine?.EngineHandle.Cancel();
        _visualizer.DisposeCurrent();
        FetchEngine = newEngine;
        return FillAsync(itemLimit);
    }

    public void Dispose()
    {
        _visualizer.DisposeCurrent();
        FetchEngine?.Cancel();
        FetchEngine = null;
    }
}