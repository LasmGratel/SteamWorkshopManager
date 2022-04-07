using System;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using SteamWorkshopManager.Client.Service;
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

    private async void RootFrame_OnLoaded(object sender, RoutedEventArgs e)
    {
        var proxy = App.Instance.AppHost.Services.GetRequiredService<IHttpProxyService>();
        proxy.ProxyDNS = IPAddress.Parse(DnsAnalysisService.PrimaryDNS_Dnspod);
        proxy.ProxyDomains = new[]
        {
            new AccelerateProjectDTO
            {
                PortId = 443,
                ServerName = "store.steampowered.com",
                DomainNames = "store.steampowered.com/app/*;store.steampowered.com/agecheck/*",
                ForwardDomainName = "partner.steamgames.com",
                Enabled = true,
                Hosts = "store.steampowered.com",
                UserAgent = "${origin} Googlebot/2.1 …www.google.com/bot.html)"
            },
            new AccelerateProjectDTO
            {
                PortId = 443,
                ServerName = "steamcommunity.com",
                DomainNames = "steamcommunity.com",
                ForwardDomainName = "partner.steamgames.com",
                Enabled = true,
                Hosts = "steamcommunity.com;www.steamcommunity.com",
            },
            new AccelerateProjectDTO
            {
                PortId = 443,
                ServerName = "steamimage.rmbgame.net",
                DomainNames = "steamcdn-a.akamaihd.net;steamuserimages-a.akamaihd.net;cdn.akamai.steamstatic.com;community.akamai.steamstatic.com",
                ForwardDomainName = "steamimage.rmbgame.net",
                Enabled = true,
                Hosts = "steamcdn-a.akamaihd.net;cdn.akamai.steamstatic.com;community.akamai.steamstatic.com",
            }
        };
        await proxy.StartProxy();
        if ((AppContext.Settings.Cookie?.Trim() ?? "") == "")
            RootFrame.Navigate(typeof(LoginPage));
        else
            RootFrame.Navigate(typeof(MainPage));
    }
}