using System.Reflection;

namespace LetopiaPlatform.Agent.Prompts;

/// <summary>
/// Loads prompt templates embedded as assembly resources.
/// </summary>
public static class PromptLoader
{
    private static readonly Lazy<string> RoadmapPrompt = new(() =>
        LoadEmbeddedPrompt("RoadmapSystemPrompt.md"));

    /// <summary>
    /// Gets the roadmap agent system prompt text.
    /// </summary>
    public static string RoadmapSystemPrompt => RoadmapPrompt.Value;

    private static string LoadEmbeddedPrompt(string fileName)
    {
        var assembly = typeof(PromptLoader).Assembly;

        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n =>
                n.EndsWith($"Prompts.{fileName}", StringComparison.Ordinal))
            ?? throw new FileNotFoundException(
                $"Embedded prompt '{fileName}' not found. " +
                $"Ensure the file exists under 'Prompts' and is set as EmbeddedResource.");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
