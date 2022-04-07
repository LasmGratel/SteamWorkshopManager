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
using SteamWorkshopManager.Client.Engine;
using SteamWorkshopManager.Core;
using SteamWorkshopManager.Model;

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is WorkshopCollection collection)
            {
                WorkshopItemGrid.ViewModel.Workshop = AppContext.CacheDatabase.GetApp(collection.AppId).Result!;
                _ = WorkshopItemGrid.ViewModel.ResetEngineAndFillAsync(new CollectionEngine(AppContext.Client, null, collection.Items));

            }
        }
    }
}
