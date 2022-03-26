using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace SteamWorkshopManager.Util.UI;

public static class UIHelper
{
    public static T GetDataContext<T>(this FrameworkElement element)
    {
        return (T)element.DataContext;
    }

    public static T GetDataContext<T>(this object element)
    {
        return ((FrameworkElement)element).GetDataContext<T>(); // direct cast will throw exception if the type check fails, and that's exactly what we want
    }
}