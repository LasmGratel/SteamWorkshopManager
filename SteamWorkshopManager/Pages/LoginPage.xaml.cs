using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Helpers;
using IdentityModel.OidcClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using SteamWorkshopManager.Client.Service;
using SteamWorkshopManager.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamWorkshopManager.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LoginPage : Page
{
    public LoginPage()
    {
        this.InitializeComponent();
    }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", $"--proxy-server={AppContext.Settings.Proxy} --ignore-certificate-errors");
        /*
        var options = new OidcClientOptions
        {
            Authority = "https://steamcommunity.com/openid",
            ClientId = "48EB70CC8F8947A16A81BA8882ACF89A",
            RedirectUri = "https://lasm.dev",
            Scope = "openid profile api",
            HttpClientFactory = clientOptions =>
                new HttpClient(new HttpClientHandler
                {
                    Proxy = new WebProxy($"http://127.0.0.1:{proxy.ProxyPort}")
                })
        };

        var client = new OidcClient(options);

        // generate start URL, state, nonce, code challenge
        var state = client.PrepareLoginAsync().Result;*/
        

        if (e.Parameter is string)
        {
            Task.Run(async () =>
            {
                await LoginWebView.CoreWebView2.ExecuteScriptAsync("document.cookie = \"" + e.Parameter + "\"");
                //LoginWebView.Source = new Uri(state.StartUrl);
                LoginWebView.Source = new Uri("https://steamcommunity.com/login/home/?l=english&goto=/app/281990");
            });
        }
        else
        {
            //LoginWebView.Source = new Uri(state.StartUrl);
            LoginWebView.Source = new Uri("https://steamcommunity.com/login/home/?l=english&goto=/app/281990");
        }
    }

    private void LoginWebView_OnNavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        LoginWebView.CoreWebView2.WebResourceResponseReceived += (view2, eventArgs) =>
        {
            var header = eventArgs.Request.Headers;
            if (eventArgs.Request.Uri == "https://steamcommunity.com/app/281990")
            {
                AppContext.Settings.Cookie = header.GetHeader("Cookie");
                Frame.Navigate(typeof(MainPage));
            }
        };
        LoginWebView.CoreWebView2.WebResourceRequested += (view2, eventArgs) =>
        {
            var header = eventArgs.Request.Headers;
        };
    }

    private void LoginWebView_OnCoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
    {
        sender.CoreWebView2.Settings.UserAgent =
            "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.7113.93 Safari/537.36";
    }
}