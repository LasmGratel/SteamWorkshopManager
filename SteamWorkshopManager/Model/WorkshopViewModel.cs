using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Model;

public class WorkshopViewModel : IDisposable
{
    public Workshop Workshop { get; }

    public SoftwareBitmap ThumbnailBitmap;

    public WorkshopViewModel(Workshop workshop)
    {
        Workshop = workshop;
        ThumbnailBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, Workshop.HeaderWidth, Workshop.HeaderHeight, BitmapAlphaMode.Ignore);
    }

    public void Dispose()
    {
        ThumbnailBitmap.Dispose();
    }
}