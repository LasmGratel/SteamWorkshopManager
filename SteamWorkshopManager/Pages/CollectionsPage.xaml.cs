using System;
using System.Collections.Generic;
using System.IO;
using Windows.Foundation.Metadata;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Model;
using SteamWorkshopManager.Model.Navigation;
using SteamWorkshopManager.Util;
using SteamWorkshopManager.Util.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamWorkshopManager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CollectionsPage : Page
    {
        public CollectionsPageViewModel ViewModel { get; }

        public CollectionsPage()
        {
            this.InitializeComponent();
            ViewModel = new CollectionsPageViewModel();
        }

        private void CollectionsPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            _ = ViewModel.Load();
        }

        private void Collection_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var model = sender.GetDataContext<WorkshopCollectionViewModel>();
            AppContext.MainNavigationController.Navigate(NavigationContext.CollectionPage(model.Collection));
        }

        private async void Import_OnClicked(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".zip");
            picker.FileTypeFilter.Add(".json");

            // Pass in the current WinUI window and get its handle
            // https://github.com/microsoft/WindowsAppSDK/issues/466
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(AppContext.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            switch (file.FileType)
            {
                case "zip":
                    var stream = await file?.OpenReadAsync();
                    if (stream != null)
                    {
                        var collection = await Parser.ParseIronyModCollection(stream.AsStreamForRead());

                        var id = AppContext.CollectionDatabase.Insert(collection);
                        ViewModel.Collections.Add(
                            new WorkshopCollectionViewModel(await AppContext.CollectionDatabase.GetCollection(id)));
                    }
                    break;
                case "json":

                    var collection1 = JsonConvert.DeserializeObject<WorkshopCollection>(await file.ReadStringAsync());

                    var id1 = AppContext.CollectionDatabase.Insert(collection1!);
                    ViewModel.Collections.Add(
                        new WorkshopCollectionViewModel(await AppContext.CollectionDatabase.GetCollection(id1)));
                    break;
            }
        }

        private async void CreateNewCollection_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateNewCollectionDialog();
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
            {
                dialog.XamlRoot = XamlRoot;
            }

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.ReloadFromDatabase();
            }
        }
    }
}
