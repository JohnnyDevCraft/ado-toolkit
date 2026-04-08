using AdoToolkit.Config;
using AdoToolkit.Models;
using AdoToolkit.Models.PullRequests;

namespace AdoToolkit.Services;

public sealed class PullRequestReviewService
{
    private readonly PullRequestStorageService _storageService;

    public PullRequestReviewService(PullRequestStorageService storageService)
    {
        _storageService = storageService;
    }

    public PullRequestThreadRecord GetThread(PullRequestSession session, int threadId)
    {
        return session.Threads.FirstOrDefault(thread => thread.ThreadId == threadId)
               ?? throw new InvalidOperationException($"Thread {threadId} was not found.");
    }

    public async Task SetDecisionAsync(AppConfig config, PullRequestSession session, int threadId, string decision, string? instruction = null, CancellationToken cancellationToken = default)
    {
        var thread = GetThread(session, threadId);
        if (!string.Equals(decision, "fix", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(decision, "no-fix", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Decision must be either `fix` or `no-fix`.");
        }

        thread.Decision = decision.ToLowerInvariant();
        thread.DeveloperNotes = string.Equals(thread.Decision, "fix", StringComparison.OrdinalIgnoreCase)
            ? instruction?.Trim()
            : null;
        thread.ReviewedAt = DateTimeOffset.UtcNow;
        await _storageService.SaveSessionAsync(config, session, cancellationToken);
    }

    public IReadOnlyList<int> GetUnreviewedThreadIds(PullRequestSession session)
    {
        return session.Threads
            .Where(thread => string.Equals(thread.Decision, "unreviewed", StringComparison.OrdinalIgnoreCase))
            .Select(thread => thread.ThreadId)
            .OrderBy(id => id)
            .ToList();
    }

    public async Task<string> GeneratePromptAsync(AppConfig config, PullRequestSession session, CancellationToken cancellationToken = default)
    {
        var unreviewed = GetUnreviewedThreadIds(session);
        if (unreviewed.Count > 0)
        {
            throw new InvalidOperationException($"Cannot generate prompt. Unreviewed threads remain: {string.Join(", ", unreviewed)}");
        }

        await _storageService.SavePromptAsync(config, session, cancellationToken);
        return session.GeneratedPrompt;
    }
}

