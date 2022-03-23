using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using CommunityToolkit.WinUI.UI.Controls;

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
        }

        private async void WorkshopItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var x = ((DockPanel)sender);
            ItemDialog.Content = await AppContext.ItemDatabase.GetItem((long)x.DataContext);
            ItemDialog.Visibility = Visibility.Visible;
            await ItemDialog.ShowAsync();
        }
    }
}
