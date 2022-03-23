using System;
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
