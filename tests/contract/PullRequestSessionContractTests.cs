using System.Text.Json;
using AdoToolkit.Models.PullRequests;

namespace AdoToolkit.Tests.Contract;

public sealed class PullRequestSessionContractTests
{
    [Fact]
    public void Pull_request_session_serializes_threads_and_prompt_fields()
    {
        var session = new PullRequestSession
        {
            Organization = "org",
            Project = "proj",
            Repository = "repo",
            PullRequestId = 12,
            PullRequestTitle = "Test PR",
            Threads =
            [
                new PullRequestThreadRecord
                {
                    ThreadId = 34,
                    Decision = "fix",
                    DeveloperNotes = "Do the thing",
                    Comments = [new PullRequestCommentRecord { ThreadId = 34, CommentId = 1, Content = "Fix this" }]
                }
            ],
            GeneratedPrompt = "Prompt text"
        };

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(session));
        Assert.True(document.RootElement.TryGetProperty("PullRequestId", out _)
                    || document.RootElement.TryGetProperty("pullRequestId", out _));
        Assert.True(document.RootElement.TryGetProperty("Threads", out _)
                    || document.RootElement.TryGetProperty("threads", out _));
        Assert.True(document.RootElement.TryGetProperty("GeneratedPrompt", out _)
                    || document.RootElement.TryGetProperty("generatedPrompt", out _));
    }
}

