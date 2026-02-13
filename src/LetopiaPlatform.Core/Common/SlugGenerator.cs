using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LetopiaPlatform.Core.Common;

/// <summary>
/// Generates URL-safe, lowecase, deterministic slugs from display names.
/// Example: "My Cool Community!" -> "my-cool-community"
/// </summary>
public static partial class SlugGenerator
{
    private static readonly Dictionary<string, string> KnownReplacements = new(StringComparer.OrdinalIgnoreCase)
    {
        ["C#"] = "csharp",
        ["C++"] = "cplusplus",
        ["F#"] = "fsharp",
        [".NET"] = "dotnet",
        ["Node.js"] = "nodejs",
        ["Vue.js"] = "vuejs",
        ["React.js"] = "reactjs",
        ["Next.js"] = "nextjs",

    };

    /// <summary>
    /// Generates a URL-safe slug from the given name.
    /// </summary>
    /// <param name="name">The display name to slugify.</param>
    /// <returns>
    /// A lowercase, hyphen-separated string containing only alphanumeric characters and hyphens.
    /// </returns>
    /// <example>
    /// <code>
    /// SlugGenerator.Generate("Backend .NET")    // returns "backend-dotnet"
    /// SlugGenerator.Generate("C# Fundamentals")   // returns "csharp-fundamentals"
    /// </code>
    /// </example>
    /// <exception cref="ArgumentException">Thrown when the input name is null or empty.</exception>
    public static string Generate(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        var result = name;

        // Replace well-known terms before lowering
        foreach(var (key, value) in KnownReplacements)
        {
            result = result.Replace(key, value, StringComparison.OrdinalIgnoreCase);
        }

        // Normalize unicode to ASCII approximation (e.g., "Café" -> "Cafe")
        result = RemoveDiacritics(result);

        // Lowercase
        result = result.ToLowerInvariant();

        // Replace non-alphanumeric characters with hyphens
        result = NonAlphanumericRegex().Replace(result, "-");

        // Collapse multiple consecutive hyphens into one
        result = MultipleHyphensRegex().Replace(result, "-");

        // Trim leading and trailing hyphens
        result = result.Trim('-');

        if (string.IsNullOrEmpty(result))
        {
            throw new ArgumentException("Name produces an empty slug.", nameof(name));
        }

        return result;
    }

    /// <summary>
    /// Generates a unique slug by appending a numeric suffix if the base slug already exists.
    /// </summary>
    /// <param name="name">The display name to slugify.</param>
    /// <param name="existsSlug">A function that checks if a slug already exists.</param>
    /// <returns>A unique slug string.</returns>
    public static async Task<string> GenerateUniqueAsync(
        string name,
        Func<string, Task<bool>> existsSlug)
    {
        var baseSlug = Generate(name);
        var candidate = baseSlug;
        var counter = 2;

        while (await existsSlug(candidate))
        {
            candidate = $"{baseSlug}-{counter}";
            counter++;
        }

        return candidate;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex("[^a-z0-9]+", RegexOptions.Compiled)]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex("-{2,}", RegexOptions.Compiled)]
    private static partial Regex MultipleHyphensRegex();
}