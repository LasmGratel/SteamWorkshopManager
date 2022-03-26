using System;
using System.Collections.Generic;

namespace SteamWorkshopManager.Util;

public static class EnumerableExtensions
{
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> enumerable)
    {
        foreach (var x in enumerable)
        {
            collection.Add(x);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var x in enumerable)
        {
            action(x);
        }
    }
}