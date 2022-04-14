using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Newtonsoft.Json;
using SteamWorkshopManager.Client.Engine;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Model;
using SteamWorkshopManager.Util;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamWorkshopManager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CollectionViewerPage : Page
    {
        public CollectionViewerPage()
        {
            this.InitializeComponent();
        }

        private WorkshopCollection _collection;

        public async Task LoadItems(WorkshopCollection collection)
        {
            WorkshopItemGrid.ViewModel.ViewModels.AddRange(
                (await Task.WhenAll(collection.Items.Select(id => AppContext.ItemDatabase.GetItem(id)).Where(x => !x.IsFaulted)))
                .Where(x => x != null)
                .Select(
                    x =>
                    {
                        return new WorkshopItemViewModel(x);
                    })
                );
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is WorkshopCollection collection)
            {

                _collection = collection;
                WorkshopItemGrid.ViewModel.Workshop = AppContext.CacheDatabase.GetApp(collection.AppId).Result!;
                _ = LoadItems(collection);

            }
        }

        private void SelectAll_OnClick(object sender, RoutedEventArgs e)
        {
            WorkshopItemGrid.ViewModel.ViewModels.ForEach(x => x.Selected = true);
            WorkshopItemGrid.ViewModel.SelectedViewModels.AddRange(WorkshopItemGrid.ViewModel.ViewModels);
        }

        private async void Export_OnClick(object sender, RoutedEventArgs e)
        {
            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("JSON File", new List<string> {".json"});
            picker.SuggestedFileName = _collection.Name + ".json";

            // Pass in the current WinUI window and get its handle
            // https://github.com/microsoft/WindowsAppSDK/issues/466
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(AppContext.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();
            await file?.WriteStringAsync(JsonConvert.SerializeObject(_collection));
        }
    }
}
