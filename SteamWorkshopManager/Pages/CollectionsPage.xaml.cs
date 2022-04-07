using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Model;
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
            var workshop = sender.GetDataContext<WorkshopCollectionViewModel>();
            Frame.NavigateToType(typeof(CollectionViewerPage), workshop.Collection, new FrameNavigationOptions());
        }

        private async void Import_OnClicked(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".zip");

            // Pass in the current WinUI window and get its handle
            // https://github.com/microsoft/WindowsAppSDK/issues/466
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(AppContext.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            var stream = await file.OpenReadAsync();
            if (stream != null)
            {
                var collection = await Parser.ParseIronyModCollection(stream.AsStreamForRead());

                var id = AppContext.CollectionDatabase.Insert(collection);
                ViewModel.Collections.Add(
                    new WorkshopCollectionViewModel(await AppContext.CollectionDatabase.GetCollection(id)));
            }
        }
    }
}
