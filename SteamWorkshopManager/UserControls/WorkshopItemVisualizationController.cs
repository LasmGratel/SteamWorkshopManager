﻿using System;
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
        if (FetchEngine == null) return false;
        var collection = new Util.IncrementalLoadingCollection<FetchEngineIncrementalSource<WorkshopItem, WorkshopItemViewModel>, WorkshopItemViewModel>(new WorkshopItemFetchEngineIncrementalSource(FetchEngine!, itemsLimit));
        _visualizer.ViewModels = collection;
        await collection.LoadMoreItemsAsync(20).AsTask().ConfigureAwait(false);
        _visualizer.ViewModels.CollectionChanged += CollectionChanged;
        return _visualizer.ViewModels.Count > 0;
    }

    public async Task FillAsync(FetchEngine<WorkshopItem?>? newEngine, int? itemsLimit = null)
    {
        FetchEngine = newEngine;
        await FillAsync(itemsLimit);
    }

    public async Task<bool> ResetAndFillAsync(FetchEngine<WorkshopItem?>? newEngine, int? itemLimit = null)
    {
        FetchEngine?.EngineHandle.Cancel();
        _visualizer.DisposeCurrent();
        FetchEngine = newEngine;
        return await FillAsync(itemLimit);
    }

    public void Dispose()
    {
        _visualizer.DisposeCurrent();
        FetchEngine = null;
    }
}