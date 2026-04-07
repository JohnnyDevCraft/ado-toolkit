using System.Text.RegularExpressions;

namespace AdoToolkit.Config;

public sealed class AppPaths
{
    private readonly string _homeDirectory;

    public AppPaths(string? homeDirectory = null)
    {
        _homeDirectory = homeDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    public string GetStorageRoot()
    {
        return Path.Combine(_homeDirectory, ".ado-toolkit");
    }

    public string GetConfigPath()
    {
        return Path.Combine(GetStorageRoot(), "config.json");
    }

    public string GetOutputRoot()
    {
        return Path.Combine(GetStorageRoot(), "outputs");
    }

    public string GetPromptRoot()
    {
        return Path.Combine(GetStorageRoot(), "prompts");
    }

    public static string Slugify(string value, string fallback = "item")
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var normalized = Regex.Replace(value.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-");
        normalized = Regex.Replace(normalized, @"-+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(normalized) ? fallback : normalized;
    }
}

