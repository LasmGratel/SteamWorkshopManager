using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamWorkshopManager.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingsPage : Page
{
    public string Cookie
    {
        get => AppContext.SessionContainer.Values["Cookie"].ToString();
        set => AppContext.SessionContainer.Values["Cookie"] = value;
    }

    public string UserLink
    {
        get => AppContext.SessionContainer.Values["UserLink"].ToString();
        set => AppContext.SessionContainer.Values["UserLink"] = value;
    }

    public SettingsPage()
    {
        this.InitializeComponent();
    }
}