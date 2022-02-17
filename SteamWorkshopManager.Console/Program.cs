// See https://aka.ms/new-console-template for more information

using System.Net;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Console;
public class Program
{
    public static async Task Main(string[] args)
    {
        var client = new SteamWorkshopClient(
            @"",
            "https://steamcommunity.com/id/",
            new HttpClientHandler
            {
                //Proxy = new WebProxy("http://127.0.0.1:1080"),
                UseCookies = false
            });
        await foreach (var item in client.GetAllSubscribedItems(255710))
        {
            System.Console.WriteLine(item);
        }
    }
}