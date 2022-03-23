using System;

namespace SteamWorkshopManager.Util;

public static class StringExtensions
{
    public static string SubstringBeforeLast(this ReadOnlySpan<char> s, char c)
    {
        return new string(s[..s.LastIndexOf(c)]);
    }

    public static string SubstringAfterLast(this ReadOnlySpan<char> s, char c)
    {
        return new string(s[(s.LastIndexOf(c) + 1)..]);
    }
}