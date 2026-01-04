using System.Text.RegularExpressions;

namespace ProductFetcher.Utils;

/// <summary>
/// Text utilities for sanitization and formatting
/// Equivalent to Python's text_utils.py
/// </summary>
public static partial class TextUtils
{
    /// <summary>
    /// Clean control characters from text that could cause issues in Excel
    /// </summary>
    public static string SanitizeString(object? text)
    {
        if (text == null)
        {
            return string.Empty;
        }

        var textStr = text.ToString() ?? string.Empty;

        // Remove ASCII control characters (except tab, newline, carriage return)
        return ControlCharsRegex().Replace(textStr, string.Empty);
    }

    [GeneratedRegex(@"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", RegexOptions.Compiled)]
    private static partial Regex ControlCharsRegex();
}
