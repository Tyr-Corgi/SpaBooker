using System.Text.RegularExpressions;

namespace SpaBooker.Core.Utilities;

/// <summary>
/// Utility class for sanitizing user inputs to prevent XSS and injection attacks
/// </summary>
public static class InputSanitizer
{
    private static readonly Regex HtmlTagRegex = new Regex(@"<[^>]*>", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
    private static readonly Regex ScriptRegex = new Regex(@"<script[^>]*>.*?</script>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    /// <summary>
    /// Removes HTML tags from input string
    /// </summary>
    public static string StripHtmlTags(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove script tags first
        var result = ScriptRegex.Replace(input, string.Empty);
        
        // Remove all other HTML tags
        result = HtmlTagRegex.Replace(result, string.Empty);
        
        return result.Trim();
    }

    /// <summary>
    /// Normalizes whitespace in input string (replaces multiple spaces with single space)
    /// </summary>
    public static string NormalizeWhitespace(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return WhitespaceRegex.Replace(input.Trim(), " ");
    }

    /// <summary>
    /// Validates that input contains only allowed characters
    /// </summary>
    public static bool ContainsOnlyAllowedCharacters(string? input, string allowedPattern = @"^[a-zA-Z0-9\s\.,!?'\-]*$")
    {
        if (string.IsNullOrEmpty(input))
            return true;

        return Regex.IsMatch(input, allowedPattern);
    }

    /// <summary>
    /// Sanitizes general text input by stripping HTML and normalizing whitespace
    /// </summary>
    public static string SanitizeTextInput(string? input, int maxLength = 500)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sanitized = StripHtmlTags(input);
        sanitized = NormalizeWhitespace(sanitized);
        
        if (sanitized.Length > maxLength)
            sanitized = sanitized.Substring(0, maxLength);

        return sanitized;
    }

    /// <summary>
    /// Sanitizes email input
    /// </summary>
    public static string SanitizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;

        return email.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Sanitizes phone number input (removes all non-digit characters except + at start)
    /// </summary>
    public static string SanitizePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;

        var cleaned = Regex.Replace(phoneNumber, @"[^\d+]", "");
        
        // Ensure + only appears at the start
        if (cleaned.Contains('+'))
        {
            cleaned = "+" + cleaned.Replace("+", "");
        }

        return cleaned;
    }

    /// <summary>
    /// Sanitizes note/comment input with more permissive character set
    /// </summary>
    public static string SanitizeNote(string? note, int maxLength = 2000)
    {
        if (string.IsNullOrWhiteSpace(note))
            return string.Empty;

        var sanitized = StripHtmlTags(note);
        sanitized = NormalizeWhitespace(sanitized);
        
        // Allow more characters for notes (including newlines, punctuation)
        sanitized = Regex.Replace(sanitized, @"[^\w\s\.,!?;:\-'""()\[\]@#\$%&*/\\+=\r\n]", "");
        
        if (sanitized.Length > maxLength)
            sanitized = sanitized.Substring(0, maxLength);

        return sanitized.Trim();
    }
}

