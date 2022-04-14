using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Client;

public static class WorkshopManagerClient
{
    public static async Task<SoftwareBitmap> GetImageAsync(this SteamWorkshopClient client, string url)
    {
        await using var stream = await client.Client.GetStreamAsync(url);
        var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
        return await decoder.GetSoftwareBitmapAsync();
    }
}