using System.Text;
using AdoToolkit.Models.PullRequests;

namespace AdoToolkit.Services;

public sealed class PullRequestPromptBuilder
{
    public string BuildFixPlanPrompt(PullRequestSession session)
    {
        var fixThreads = session.Threads
            .Where(thread => string.Equals(thread.Decision, "fix", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine("These are comments found on an Azure DevOps pull request.");
        builder.AppendLine("I want you to review them and produce a concrete implementation plan for fixing them.");
        builder.AppendLine("Show me the plan first.");
        builder.AppendLine("Do not implement anything until I approve the plan.");
        builder.AppendLine();
        builder.AppendLine($"Organization: {session.Organization}");
        builder.AppendLine($"Project: {session.Project}");
        builder.AppendLine($"Repository: {session.Repository}");
        builder.AppendLine($"Pull Request ID: {session.PullRequestId}");
        builder.AppendLine($"Pull Request Title: {session.PullRequestTitle}");
        builder.AppendLine();

        if (fixThreads.Count == 0)
        {
            builder.AppendLine("No threads are currently marked as fix.");
            return builder.ToString().TrimEnd();
        }

        builder.AppendLine("Threads to fix:");
        builder.AppendLine();
        foreach (var thread in fixThreads)
        {
            builder.AppendLine($"- Thread ID: {thread.ThreadId}");
            builder.AppendLine($"  File Path: {thread.FilePath ?? "Not provided"}");
            builder.AppendLine($"  Line: {FormatLine(thread.Line, thread.EndLine)}");
            builder.AppendLine($"  Status: {thread.Status ?? "Unknown"}");

            if (!string.IsNullOrWhiteSpace(thread.DeveloperNotes))
            {
                builder.AppendLine("  Developer Notes:");
                builder.AppendLine($"  {NormalizeMultiline(thread.DeveloperNotes)}");
            }

            builder.AppendLine("  Thread Comments:");
            foreach (var comment in thread.Comments)
            {
                builder.AppendLine($"    - Author: {comment.Author ?? "Unknown"}");
                builder.AppendLine($"      Comment Type: {comment.CommentType ?? "Unknown"}");
                builder.AppendLine($"      Published: {comment.PublishedDate?.ToString("u") ?? "Unknown"}");
                builder.AppendLine("      Comment:");
                builder.AppendLine($"      {NormalizeMultiline(comment.Content)}");
            }

            if (!string.IsNullOrWhiteSpace(thread.CodeExcerpt))
            {
                builder.AppendLine("  Code Excerpt:");
                builder.AppendLine($"  {NormalizeMultiline(thread.CodeExcerpt)}");
            }

            builder.AppendLine();
        }

        builder.AppendLine("After I approve the plan, implement the approved plan.");
        return builder.ToString().TrimEnd();
    }

    private static string FormatLine(int? line, int? endLine)
    {
        if (line is null) return "Not provided";
        return endLine is not null && endLine != line ? $"{line}-{endLine}" : line.Value.ToString();
    }

    private static string NormalizeMultiline(string content) => content.Replace(Environment.NewLine, $"{Environment.NewLine}  ");
}

