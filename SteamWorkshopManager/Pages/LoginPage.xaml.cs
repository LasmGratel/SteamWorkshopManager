using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.OidcClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using SteamWorkshopManager.Client.Service;

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



    private async Task CoreWebView2OnDOMContentLoaded(CoreWebView2 sender, CoreWebView2DOMContentLoadedEventArgs args)
    {
        if (LoginWebView.Source.ToString().Contains("myworkshopfiles"))
        {
            // Login complete
            AppContext.SessionContainer.Values["Cookie"] = (await LoginWebView.CoreWebView2.ExecuteScriptAsync("document.cookie"))[1..^1]; // Fucking tailing quote

            var userLink = await LoginWebView.CoreWebView2.ExecuteScriptAsync(@"jQuery(""a.user_avatar"").attr(""href"")");
            

            AppContext.SessionContainer.Values["UserLink"] = userLink[1..^2]; // Remove tailing slash and quote
            Frame.Navigate(typeof(MainPage));
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var proxy = App.Instance.AppHost.Services.GetRequiredService<IHttpProxyService>();
        Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", $"--proxy-server=127.0.0.1:{proxy.ProxyPort} --ignore-certificate-errors");
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
                LoginWebView.Source = new Uri("https://steamcommunity.com/login/home/?l=english&goto=%2Fid%2F123%2Fmyworkshopfiles%3Fbrowsefilter%3Dmysubscriptions%26sortmethod%3Dsubscriptiondate%26section%3Ditems%26appid%3D255710%26p%3D1%26numperpage%3D10");
            });
        }
        else
        {
            //LoginWebView.Source = new Uri(state.StartUrl);
            LoginWebView.Source = new Uri("https://steamcommunity.com/login/home/?l=english&goto=%2Fid%2F123%2Fmyworkshopfiles%3Fbrowsefilter%3Dmysubscriptions%26sortmethod%3Dsubscriptiondate%26section%3Ditems%26appid%3D255710%26p%3D1%26numperpage%3D10");
        }
    }

    private void LoginWebView_OnNavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        LoginWebView.CoreWebView2.DOMContentLoaded += (view2, args) =>
        {
            _ = CoreWebView2OnDOMContentLoaded(view2, args);
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