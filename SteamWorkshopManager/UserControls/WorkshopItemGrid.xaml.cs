using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using SteamWorkshopManager.Client.Engine;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Model;
using SteamWorkshopManager.Util;
using SteamWorkshopManager.Util.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamWorkshopManager.UserControls
{
    public sealed partial class WorkshopItemGrid : UserControl
    {
        public WorkshopItemGridViewModel ViewModel { get; }

        public WorkshopItemGrid()
        {
            this.InitializeComponent();
            ViewModel = new WorkshopItemGridViewModel();
        }

        private void RefreshItems_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void CheckBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var model = sender.GetDataContext<WorkshopItemViewModel>();
            if (model.Selected)
            {
                ViewModel.SelectedViewModels.Add(model);
            }
            else
            {
                ViewModel.SelectedViewModels.Remove(model);
            }
            RefreshUnselectAllLabel();
        }

        private async void WorkshopItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            ItemDialog.Content = new WorkshopItemDetailsViewModel(await AppContext.ItemDatabase.GetItem(sender.GetDataContext<WorkshopItemViewModel>().Item.Id));
            ItemDialog.Visibility = Visibility.Visible;
            await ItemDialog.ShowAsync();
        }

        private void RefreshUnselectAllLabel()
        {
            var count = ViewModel.SelectedViewModels.Count;
            UnselectAll.Label = count > 0 ? $"Unselect All ({count})" : "Unselect All";
        }

        private void UnselectAll_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearSelection();
            RefreshUnselectAllLabel();
        }

        private async void Subscribe_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var model = sender.GetDataContext<WorkshopItemViewModel>();
            await model.ToggleSubscribe().ConfigureAwait(false);
        }

        private async void Favorite_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            await sender.GetDataContext<WorkshopItemViewModel>().ToggleFavorite();
        }

        private void SearchBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            UpdateSearch(args.QueryText);
        }

        private void SearchBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            UpdateSearch(args.SelectedItem.ToString());
        }

        private void UpdateSearch(string? criteria)
        {
            ViewModel.SearchContext.AppId = ViewModel.Workshop.AppId;
            ViewModel.SearchContext.SearchText = criteria ?? "";
            _ = ViewModel.ResetEngineAndFillAsync(new SearchEngine(AppContext.Client, null, ViewModel.SearchContext));
        }

        private void ToggleTags_OnClick(object sender, RoutedEventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        }

        private void SearchBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            SearchBox.ItemsSource = ViewModel.SearchTree.Search(sender.Text.ToLower()).Take(10)
                .Select(i => ViewModel.ViewModels[i].Item.Name);
        }

        private async void SubscribeItems_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var viewModel in ViewModel.SelectedViewModels)
            {
                await viewModel.Subscribe();
            }

            ViewModel.ClearSelection();
        }

        private async void FavoriteItems_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var viewModel in ViewModel.SelectedViewModels)
            {
                await viewModel.Favorite();
            }

            ViewModel.ClearSelection();
        }

        private void SubmitTags_OnClicked(object sender, RoutedEventArgs e)
        {
            var tags = ViewModel.Tags.Where(x => x.Selected).Select(x => x.Tag.Name);
            ViewModel.SearchContext.AppId = ViewModel.Workshop.AppId;
            ViewModel.SearchContext.Tags.Clear();
            ViewModel.SearchContext.Tags.AddRange(tags);
            _ = ViewModel.ResetEngineAndFillAsync(new SearchEngine(AppContext.Client, null, ViewModel.SearchContext));
        }

        private void ClearTags_OnClicked(object sender, RoutedEventArgs e)
        {
            
            var tags = ViewModel.Tags.Where(x => x.Selected);
            if (tags.Any())
            {
                ViewModel.SearchContext.AppId = ViewModel.Workshop.AppId;
                ViewModel.SearchContext.Tags.Clear();
            }
            tags.ForEach(x => x.Selected = false);
        }

        private async void MarkdownTextBlock_OnLinkClicked(object? sender, LinkClickedEventArgs e)
        {
            if (Uri.TryCreate(e.Link, UriKind.Absolute, out var link))
            {
                await Launcher.LaunchUriAsync(link);
            }
        }

        private void WorkshopItem_OnEffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
        {
            var model = sender.GetDataContext<WorkshopItemViewModel>();
            if (ViewModel.ItemsView.Count > 0 && Equals(model, ViewModel.ItemsView.Last()))
            {
                var y = args.BringIntoViewDistanceY;
                if (y <= sender.ActualHeight * 2)
                {
                    _ = ViewModel.LoadMoreItemsAsync(50);
                }
            }
        }

        public List<SortOptions> SortOptionsList => new(Enum.GetValues<SortOptions>());
        private SortOptions _currentSortOptions = SortOptions.Trend;

        private void SortOptions_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.SearchContext.SortOptions != _currentSortOptions)
            {
                _currentSortOptions = ViewModel.SearchContext.SortOptions;
                ViewModel.SearchContext.AppId = ViewModel.Workshop.AppId;
                _ = ViewModel.ResetEngineAndFillAsync(new SearchEngine(AppContext.Client, null, ViewModel.SearchContext));
            }
        }
    }
}
