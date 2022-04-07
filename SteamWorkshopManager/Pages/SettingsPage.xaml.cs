using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamWorkshopManager.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingsPage : Page
{
    public string? Cookie
    {
        get => AppContext.Settings.Cookie;
        set => AppContext.Settings.Cookie = value;
    }

    public string? UserLink
    {
        get => AppContext.Settings.UserLink;
        set => AppContext.Settings.UserLink = value;
    }

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    private void RefreshLogin_OnClick(object sender, RoutedEventArgs e)
    {
        AppContext.MainWindow.RootFrame.Navigate(typeof(LoginPage), AppContext.Settings.Cookie);

    }

    private void LogoutButton_OnClick(object sender, RoutedEventArgs e)
    {
        AppContext.SessionContainer.Values.Remove("Cookie");
        AppContext.SessionContainer.Values.Remove("UserLink");
        AppContext.MainWindow.RootFrame.Navigate(typeof(LoginPage), "");
    }
}