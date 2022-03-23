﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Util;
using SuffixTreeSharp;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamWorkshopManager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppsPage : Page
    {
        public ObservableCollection<Workshop> AppsCollection = new();
        public AdvancedCollectionView AppsCollectionView { get; set; }

        public GeneralizedSuffixTree SearchTree = new();

        public ObservableCollection<Workshop> FilteredApps { get; set; } = new();

        public AppsPage()
        {
            AppsCollectionView = new AdvancedCollectionView(AppsCollection)
            {
                Filter = o => FilteredApps.Count == 0 || FilteredApps.Contains(o as Workshop)
            };
            this.InitializeComponent();
        }

        private void RefreshApps_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _ = RefreshApps();
        }

        private async Task RefreshApps()
        {
            await AppContext.CacheDatabase.RefreshApps();
            AppsCollection.Clear();
            AppsCollection.AddRange(AppContext.CacheDatabase.GetApps());
        }

        private void App_OnTapped(object sender, TappedRoutedEventArgs e)
        {
        }


        private void AppsPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            AppsCollection.Clear();
            AppsCollection.AddRange(AppContext.CacheDatabase.GetApps());
            for (var i = 0; i < AppsCollection.Count; i++)
            {
                SearchTree.Put(AppsCollection[i].Name.ToLower(), i); 
            }
        }

        private void AutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            FilteredApps.Clear();
            if (sender.Text.Length > 0)
                FilteredApps.AddRange(SearchTree.Search(sender.Text.ToLower()).Select(x => AppsCollection[x]));

            AppsCollectionView.RefreshFilter();
        }

        private void AutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = args.SelectedItem.ToString();
        }

        private void AutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
        {
            AutoSuggestBox.IsSuggestionListOpen = true;
        }

        private void AutoSuggestBox_LostFocus(object sender, RoutedEventArgs e)
        {
            AutoSuggestBox.IsSuggestionListOpen = false;
        }
    }
}