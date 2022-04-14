using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using SteamWorkshopManager.Model.Navigation;
using SteamWorkshopManager.Util.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamWorkshopManager.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    public static MainPage Instance;

    private ObservableCollection<NavigationContext> MainMenuItems { get; } = new();

    private ObservableCollection<NavigationContext> FooterMenuItems { get; } = new();

    public NavigationController NavigationController { get; }

    public MainPage()
    {
        this.InitializeComponent();
        NavigationController = new NavigationController(NavView, ContentFrame, context =>
        {
            MainMenuItems.Add(context);
        });

        AppContext.MainNavigationController = NavigationController;

        Instance = this;

        MainMenuItems.Add(NavigationContext.AppsPage);
        MainMenuItems.Add(NavigationContext.CollectionsPage);
        FooterMenuItems.Add(NavigationContext.SettingsPage);
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        if (AppContext.Settings.UserLink?.Equals("") ?? true)
        {
            _ = Task.Run(async () =>
            {
                AppContext.Settings.UserLink = await AppContext.Client.GetUserLink();
            });
        }
    }

    private void NavView_BackRequested(NavigationView sender,
        NavigationViewBackRequestedEventArgs args)
    {
        TryGoBack();
    }

    private bool TryGoBack()
    {
        if (!ContentFrame.CanGoBack)
            return false;

        // Don't go back if the nav pane is overlayed.
        if (NavView.IsPaneOpen &&
            (NavView.DisplayMode == NavigationViewDisplayMode.Compact ||
             NavView.DisplayMode == NavigationViewDisplayMode.Minimal))
            return false;

        ContentFrame.GoBack();
        return true;
    }

    private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        NavigationController.Navigate(NavigationContext.AppsPage);
    }

    private void NavigationViewItem_MiddleClicked(object sender, PointerRoutedEventArgs e)
    {
        var pointer = e.GetCurrentPoint(sender as UIElement);
        if (pointer.Properties.PointerUpdateKind == Microsoft.UI.Input.PointerUpdateKind.MiddleButtonPressed)
        {
            e.Handled = true;

            if (sender.GetDataContext<NavigationContext>() is { } context)
            {
                MainMenuItems.Remove(context);
            }
        }
    }

    private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        if (sender.GetDataContext<NavigationContext>() is { } context)
        {
            MainMenuItems.Remove(context);
        }
    }

    private void NavViewItem_Loaded(object sender, RoutedEventArgs e)
    {
        // Select navigation item
        if (sender.GetDataContext<NavigationContext>().Equals(NavigationController.CurrentContext))
            NavView.SelectedItem = sender;
    }
}