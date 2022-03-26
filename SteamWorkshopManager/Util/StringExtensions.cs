using System;
using System.Text.RegularExpressions;

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

    public static bool IsDomainPattern(this string str, string pattern, RegexOptions options = RegexOptions.IgnoreCase)
        => Regex.IsMatch(str, Regex.Escape(pattern)
            .Replace(@"\*", @"[^\.]*"), options);

    public static bool IsHttpUrl(this string? url, bool httpsOnly = false) => url != null &&
        (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
         (!httpsOnly && url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)));

    /// <summary>
    /// 获取两个字符串中间的字符串
    /// </summary>
    /// <param name="s">要处理的字符串，例 ABCD</param>
    /// <param name="left">第1个字符串，例 AB</param>
    /// <param name="right">第2个字符串，例 D</param>
    /// <param name="isContains">是否包含标志字符串</param>
    /// <returns>例 返回C</returns>
    public static string Substring(this string s, string left, string right, bool isContains = false)
    {
        int i1 = s.IndexOf(left);
        if (i1 < 0) // 找不到返回空
        {
            return "";
        }

        int i2 = s.IndexOf(right, i1 + left.Length); // 从找到的第1个字符串后再去找
        if (i2 < 0) // 找不到返回空
        {
            return "";
        }

        if (isContains) return s.Substring(i1, i2 - i1 + left.Length);
        else return s.Substring(i1 + left.Length, i2 - i1 - left.Length);
    }
}