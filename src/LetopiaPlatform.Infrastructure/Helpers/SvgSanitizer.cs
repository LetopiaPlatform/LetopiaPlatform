using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace LetopiaPlatform.Infrastructure.Helpers;

public static class SvgSanitizer
{
    private static readonly Regex EventHandlerRegex = new(
        @"\bon(load|click|error|mouseover|mouseout|mousedown|mouseup|mousemove|focus|blur|submit|change|input|keydown|keyup|keypress|touchstart|touchend|touchmove|dragstart|drag|drop|scroll|resize|unload|beforeunload|abort|contextmenu|dblclick|pointerdown|pointerup)\s*=",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromSeconds(1));

    /// <summary>
    /// Validates SVG content for unsafe elements. Resets the file stream position after reading.
    /// </summary>
    public static string? Validate(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, leaveOpen: true);
        var content = reader.ReadToEnd();
        stream.Position = 0;

        var lower = content.ToLowerInvariant();

        if (lower.Contains("<script")
            || lower.Contains("javascript:")
            || lower.Contains("<foreignobject")
            || lower.Contains("data:text/html")
            || EventHandlerRegex.IsMatch(lower))
        {
            return "SVG contains potentially unsafe content (scripts or event handlers).";
        }

        return null;
    }
}