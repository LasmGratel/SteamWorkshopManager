using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SteamWorkshopManager.Pages;
using SteamWorkshopManager.Util.UI;

namespace SteamWorkshopManager.Model.Navigation;

public class NavigationController
{
    public NavigationView NavigationView { get; }

    public Frame NavigationFrame { get; }

    public AddNavigationItem OnAddNavigationItem { get; }

    public NavigationContext CurrentContext { get; set; }

    public NavigationController(NavigationView navigationView, Frame navigationFrame, AddNavigationItem onAddNavigationItem)
    {
        NavigationView = navigationView;
        NavigationFrame = navigationFrame;
        OnAddNavigationItem = onAddNavigationItem;
        //NavigationView.ItemInvoked += NavigationViewOnItemInvoked;
        NavigationView.SelectionChanged += NavigationViewOnSelectionChanged;
        NavigationFrame.Navigated += NavigationFrameOnNavigated;
    }

    private void NavigationFrameOnNavigated(object sender, NavigationEventArgs e)
    {
        NavigationView.IsBackEnabled = NavigationFrame.CanGoBack;
    }

    public delegate void AddNavigationItem(NavigationContext context);

    private void NavigationViewOnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            if (NavigationFrame.CurrentSourcePageType != typeof(SettingsPage))
                NavigationFrame.Navigate(typeof(SettingsPage));
        }
        else if (args.SelectedItemContainer != null)
        {
            Navigate(args.SelectedItemContainer.GetDataContext<NavigationContext>());
        }
    }

    private void NavigationViewOnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            if (NavigationFrame.CurrentSourcePageType != typeof(SettingsPage))
                NavigationFrame.Navigate(typeof(SettingsPage));
        }
        else if (args.InvokedItemContainer != null)
        {
            Navigate(args.InvokedItemContainer.GetDataContext<NavigationContext>());
        }
    }

    public void Navigate(NavigationContext context)
    {
        // Find the item in menu items
        if (NavigationView.MenuItemsSource is ICollection<NavigationContext> items && !items.Contains(context)
            && NavigationView.FooterMenuItemsSource is ICollection<NavigationContext> items2 && !items2.Contains(context))
        {
            OnAddNavigationItem(context); // If not, add it to collection
        }

        CurrentContext = context;

        // Actual navigation
        NavigationFrame.Navigate(context.PageType, context.Parameter);
    }
}