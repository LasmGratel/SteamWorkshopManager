using Microsoft.UI.Xaml;
using SteamWorkshopManager.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamWorkshopManager;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        AppContext.MainWindow = this;
        this.InitializeComponent();
    }

    private void RootFrame_OnLoaded(object sender, RoutedEventArgs e)
    {
        if ((AppContext.SessionContainer.Values["Cookie"]?.ToString()?.Trim() ?? "") == "")
            RootFrame.Navigate(typeof(LoginPage));
        else
            RootFrame.Navigate(typeof(MainPage));
    }
}