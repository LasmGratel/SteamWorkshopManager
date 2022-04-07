namespace SteamWorkshopManager.Core;

public enum SortOptions
{
    Trend, MostRecent, LastUpdated, TotalUniqueSubscribers
}

public static class SortOptionsExtensions
{
    public static string GetName(this SortOptions sortOptions)
    {
        return Enum.GetName(sortOptions)!.ToLower();
    }

    public static string ToString(this SortOptions sortOptions)
    {
        return Enum.GetName(sortOptions)!;
    }
}