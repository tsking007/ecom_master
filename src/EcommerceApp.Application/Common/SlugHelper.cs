using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EcommerceApp.Application.Common;

/// <summary>
/// Converts any string into a URL-safe lowercase slug.
/// Used when creating products to generate the slug from the product name.
///
/// Example:
///   "Apple iPhone 15 Pro (128GB)" → "apple-iphone-15-pro-128gb"
///   "100% Merino Wool Sweater!"   → "100-merino-wool-sweater"
/// </summary>
public static class SlugHelper
{
    private static readonly Regex _whitespace = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex _invalidChars = new(@"[^a-z0-9\-]", RegexOptions.Compiled);
    private static readonly Regex _multipleHyphen = new(@"-{2,}", RegexOptions.Compiled);

    public static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // 1. Normalize Unicode characters to ASCII equivalents
        //    e.g. "café" → "cafe", "über" → "uber"
        var normalized = input.Normalize(NormalizationForm.FormKD);
        var ascii = new StringBuilder();

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
                ascii.Append(c);
        }

        var slug = ascii.ToString().ToLowerInvariant();

        // 2. Replace whitespace runs with a single hyphen
        slug = _whitespace.Replace(slug, "-");

        // 3. Remove any character that is not a lowercase letter, digit, or hyphen
        slug = _invalidChars.Replace(slug, string.Empty);

        // 4. Collapse consecutive hyphens into one
        slug = _multipleHyphen.Replace(slug, "-");

        // 5. Strip leading and trailing hyphens
        slug = slug.Trim('-');

        return slug;
    }

    /// <summary>
    /// Appends a numeric suffix to make a slug unique.
    /// Called when the base slug already exists in the database.
    ///
    /// Example: "iphone-15" already exists → returns "iphone-15-2"
    /// </summary>
    public static string AppendSuffix(string slug, int suffix)
        => $"{slug}-{suffix}";
}