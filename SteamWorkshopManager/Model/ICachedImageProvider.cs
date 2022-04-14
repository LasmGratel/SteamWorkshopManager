using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace SteamWorkshopManager.Model;

/// <summary>
/// Provide a async loadable and cached SoftwareBitmap
/// </summary>
public interface ICachedImageProvider : IDisposable
{
    public SoftwareBitmap Image { get; set; }
    
}