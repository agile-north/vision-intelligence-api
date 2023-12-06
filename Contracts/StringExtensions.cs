using System;
using System.Text.RegularExpressions;

public static class StringExtensions
{
    internal static bool IsBase64(this string s)
    {
        if (string.IsNullOrEmpty(s) || s.Length % 4 != 0)
            return false;

        // Pattern breakdown:
        // ^(?:[A-Za-z0-9+/]{4})*: Matches blocks of 4 valid Base64 characters.
        // (?:[A-Za-z0-9+/]{2}==): Matches Base64 character block ending in '=='
        // (?:[A-Za-z0-9+/]{3}=): Matches Base64 character block ending in '='
        string base64Pattern = @"^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$";

        if (!Regex.IsMatch(s, base64Pattern))
            return false;

        // Attempt to decode the string
        try
        {
            Convert.FromBase64String(s);
            return true;
        }
        catch
        {
            return false; // Decoding failed means it's not a valid Base64 string
        }
    }
}