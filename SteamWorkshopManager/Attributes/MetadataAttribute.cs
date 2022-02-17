using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamWorkshopManager.Util;

namespace SteamWorkshopManager.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class Metadata : Attribute
{
    public Metadata(string key)
    {
        Key = key;
    }

    public string Key { get; }
}

public static class MetadataAttributeHelper
{
    public static string? GetMetadataOnEnumMember(this Enum e)
    {
        return e.GetCustomAttribute<Metadata>()?.Key;
    }
}
