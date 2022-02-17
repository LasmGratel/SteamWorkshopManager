using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SteamWorkshopManager.Util;
public static class AttributeHelper
{
    public static TAttribute? GetCustomAttribute<TAttribute>(this Enum e) where TAttribute : Attribute
    {
        return e.GetType().GetField(e.ToString())?.GetCustomAttribute(typeof(TAttribute), false) as TAttribute;
    }
}