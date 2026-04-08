using AdoToolkit.Models.PullRequests;

namespace AdoToolkit.Services;

public sealed class PullRequestCodeExcerptService
{
    public void PopulateThreadExcerpts(PullRequestSession session, string repoPath, int contextLines = 5)
    {
        foreach (var thread in session.Threads)
        {
            PopulateThreadExcerpt(thread, repoPath, contextLines);
        }
    }

    public void PopulateThreadExcerpt(PullRequestThreadRecord thread, string repoPath, int contextLines = 5)
    {
        thread.CodeExcerpt = null;
        thread.CodeExcerptStartLine = null;
        thread.CodeExcerptLanguage = null;

        if (string.IsNullOrWhiteSpace(repoPath) || string.IsNullOrWhiteSpace(thread.FilePath) || thread.Line is null)
        {
            return;
        }

        var relativePath = thread.FilePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
        var absolutePath = Path.Combine(repoPath, relativePath);
        if (!File.Exists(absolutePath))
        {
            return;
        }

        var allLines = File.ReadAllLines(absolutePath);
        if (allLines.Length == 0)
        {
            return;
        }

        var targetIndex = Math.Max(thread.Line.Value - 1, 0);
        var start = Math.Max(0, targetIndex - contextLines);
        var end = Math.Min(allLines.Length - 1, targetIndex + contextLines);

        thread.CodeExcerptStartLine = start + 1;
        thread.CodeExcerpt = string.Join(Environment.NewLine, allLines.Skip(start).Take(end - start + 1));
        thread.CodeExcerptLanguage = InferLanguageFromPath(absolutePath);
    }

    private static string InferLanguageFromPath(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".cs" => "csharp",
            ".csproj" => "xml",
            ".json" => "json",
            ".js" => "javascript",
            ".ts" => "typescript",
            ".tsx" => "tsx",
            ".jsx" => "jsx",
            ".php" => "php",
            ".html" => "html",
            ".css" => "css",
            ".scss" => "scss",
            ".md" => "markdown",
            ".xml" => "xml",
            ".yml" or ".yaml" => "yaml",
            ".sql" => "sql",
            ".sh" => "bash",
            _ => "txt"
        };
    }
}

