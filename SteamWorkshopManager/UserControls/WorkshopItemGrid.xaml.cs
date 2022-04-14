using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation.Metadata;
using Windows.Graphics.Imaging;
using Windows.System;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using SteamWorkshopManager.Client.Engine;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Model;
using SteamWorkshopManager.Model.Navigation;
using SteamWorkshopManager.Pages;
using SteamWorkshopManager.Util;
using SteamWorkshopManager.Util.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamWorkshopManager.UserControls
{
    public sealed partial class WorkshopItemGrid : UserControl, IDisposable
    {
        public WorkshopItemGridViewModel ViewModel { get; }

        public WorkshopItemGrid()
        {
            PrimaryCommands = new List<ICommandBarElement>();
            SecondaryCommands = new List<ICommandBarElement>();
            this.InitializeComponent();
            if (Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent("Windows.UI.Xaml.FrameworkElement", "AllowFocusOnInteraction"))
                AddToCollectionButton.AllowFocusOnInteraction = true;
            ViewModel = new WorkshopItemGridViewModel();
        }

        public static readonly DependencyProperty PrimaryCommandsProperty = DependencyProperty.Register(
            "PrimaryCommands", typeof(List<ICommandBarElement>), typeof(WorkshopItemGrid), new PropertyMetadata(default(List<ICommandBarElement>)));

        public List<ICommandBarElement> PrimaryCommands
        {
            get { return (List<ICommandBarElement>) GetValue(PrimaryCommandsProperty); }
            set { SetValue(PrimaryCommandsProperty, value); }
        }

        public static readonly DependencyProperty SecondaryCommandsProperty = DependencyProperty.Register(
            "SecondaryCommands", typeof(List<ICommandBarElement>), typeof(WorkshopItemGrid), new PropertyMetadata(default(List<ICommandBarElement>)));

        public List<ICommandBarElement> SecondaryCommands
        {
            get { return (List<ICommandBarElement>) GetValue(SecondaryCommandsProperty); }
            set { SetValue(SecondaryCommandsProperty, value); }
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

        private void WorkshopItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var model = sender.GetDataContext<WorkshopItemViewModel>();
            if (ViewModel.HasItemSelected)
            {
                model.Selected = !model.Selected;
                CheckBox_Tapped(sender, e);
            }
            else
            {
                var item = model.Item;
                AppContext.MainNavigationController.Navigate(NavigationContext.ItemPage(item.Id, item.Name));
            }
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
            ViewModel.ShowInfo = true;
            ViewModel.InfoTitle = "Progressing";
            ViewModel.InfoMessage = $"Subscription to {ViewModel.SelectedViewModels.Count} items";
            ViewModel.InfoMaxProgress = ViewModel.SelectedViewModels.Count;
            ViewModel.InfoProgress = 0;

            ViewModel.Severity = InfoBarSeverity.Informational;

            foreach (var viewModel in ViewModel.SelectedViewModels)
            {
                await viewModel.Subscribe();
                ViewModel.InfoProgress++;
            }

            ViewModel.InfoCompleted = true;
            ViewModel.InfoTitle = "Completed";
            ViewModel.Severity = InfoBarSeverity.Success;

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

        private void WorkshopItemGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            CommandBar.PrimaryCommands.AddRange(PrimaryCommands);
            CommandBar.SecondaryCommands.AddRange(SecondaryCommands);
        }

        public void Dispose()
        {
            ViewModel.Dispose();
        }

        private void AddToCollection_OnClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedCollection is { } collection)
            {
                collection.Items.AddRange(ViewModel.SelectedViewModels.Select(x => x.Item.Id));
                AppContext.CollectionDatabase.InsertOrUpdate(collection);
            }
        }

        private async void CreateNewCollection_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateNewCollectionDialog(ViewModel.Workshop);
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
            {
                dialog.XamlRoot = XamlRoot;
            }

            await dialog.ShowAsync();
        }
        
        private void SelectCollection_DropDownOpened(object? sender, object e)
        {
        }

        private void SelectCollection_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            //ViewModel.SelectedCollection = (WorkshopCollection?) ((ComboBox) sender).SelectedItem;
        }

        private void SelectCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void SelectCollection_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Collections.Clear();
            ViewModel.Collections.AddRange(AppContext.CollectionDatabase.GetCollectionsForApp(ViewModel.Workshop.AppId));
        }
    }
}
