using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using SteamWorkshopManager.Client.Engine;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Model;

namespace SteamWorkshopManager.UserControls;

public partial class WorkshopItemGridViewModel : ObservableObject, IDisposable, IWorkshopItemVisualizer
{
    private readonly WorkshopItemVisualizationController _controller;

    [ObservableProperty]
    private bool _hasNoItems;

    private ObservableCollection<WorkshopItemViewModel> _viewModels;

    public void DisposeCurrent()
    {
        _controller.FetchEngine?.Cancel();
        foreach (var model in _viewModels)
        {
            model.Dispose();
        }

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
        }
    }

    public AdvancedCollectionView ItemsView { get; set; }

    public WorkshopItemGridViewModel()
    {
        _viewModels = new ObservableCollection<WorkshopItemViewModel>();
        _controller = new WorkshopItemVisualizationController(this);
        ItemsView = new AdvancedCollectionView(ViewModels);
    }

    public async Task ResetEngineAndFillAsync(FetchEngine<WorkshopItem?>? newEngine, int? itemLimit = null)
    {
        HasNoItems = !await _controller.ResetAndFillAsync(newEngine, itemLimit);
    }

    public void Dispose()
    {
        _controller.Dispose();
        GC.SuppressFinalize(this);
    }
}