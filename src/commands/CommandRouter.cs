using AdoToolkit.Config;
using AdoToolkit.Models.PullRequests;
using AdoToolkit.Presentation;
using AdoToolkit.Services;

namespace AdoToolkit.Commands;

public sealed class CommandRouter
{
    private readonly AppHost _host;
    private readonly HelpCommand _helpCommand;
    private readonly SetupCommand _setupCommand;
    private readonly ConfigCommands _configCommands;
    private readonly WorkItemCommands _workItemCommands;
    private readonly WorkItemHistoryCommands _workItemHistoryCommands;
    private readonly PullRequestCommands _pullRequestCommands;
    private readonly PullRequestHistoryCommands _pullRequestHistoryCommands;
    private readonly MainMenuWorkflow _mainMenuWorkflow;

    public CommandRouter(AppHost host)
    {
        _host = host;
        _helpCommand = new HelpCommand();
        _setupCommand = new SetupCommand(host.SetupWorkflowService);
        _configCommands = new ConfigCommands(host.CurrentContextService);
        _workItemCommands = new WorkItemCommands(host.WorkItemRetrievalService, host.WorkItemArtifactWriter, host.JsonFileStore, host.WorkItemIndexService);
        _workItemHistoryCommands = new WorkItemHistoryCommands(host.ArtifactViewerService);
        _pullRequestCommands = new PullRequestCommands(host.PullRequestImportService, host.PullRequestStorageService, host.PullRequestReviewService, host.ArtifactViewerService);
        _pullRequestHistoryCommands = new PullRequestHistoryCommands(host.ConfigService, host.PullRequestStorageService, host.ArtifactViewerService);
        _mainMenuWorkflow = host.MainMenuWorkflow;
    }

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        _host.HeaderRenderer.Render();

        try
        {
            if (args.Length == 0)
            {
                var initialConfig = await _host.ConfigService.LoadOrCreateAsync(cancellationToken);
                await _mainMenuWorkflow.RunAsync(initialConfig, cancellationToken);
                return 0;
            }

            if (args[0].Equals("--help", StringComparison.OrdinalIgnoreCase) || args[0].Equals("-h", StringComparison.OrdinalIgnoreCase))
            {
                _helpCommand.Execute();
                return 0;
            }

            if (args[0].Equals("--version", StringComparison.OrdinalIgnoreCase) || args[0].Equals("-v", StringComparison.OrdinalIgnoreCase))
            {
                _host.Output.WriteInfo("ado 0.1.0");
                return 0;
            }

            var config = await _host.ConfigService.LoadOrCreateAsync(cancellationToken);

            if (args[0].Equals("setup", StringComparison.OrdinalIgnoreCase))
            {
                await _setupCommand.ExecuteAsync(config, cancellationToken);
                _host.Output.WriteSuccess("Setup completed.");
                return 0;
            }

            if (args.Length >= 2 && args[0].Equals("config", StringComparison.OrdinalIgnoreCase))
            {
                return await HandleConfigAsync(config, args[1..], cancellationToken);
            }

            if (args.Length >= 2 && args[0].Equals("work-item", StringComparison.OrdinalIgnoreCase))
            {
                return await HandleWorkItemAsync(config, args[1..], cancellationToken);
            }

            if (args.Length >= 2 && args[0].Equals("pr", StringComparison.OrdinalIgnoreCase))
            {
                return await HandlePullRequestAsync(config, args[1..], cancellationToken);
            }

            throw new InvalidOperationException("Unknown command. Run `ado --help` to see supported commands.");
        }
        catch (Exception ex)
        {
            _host.Output.WriteError(ex.Message);
            return 1;
        }
    }

    private async Task<int> HandleConfigAsync(Models.AppConfig config, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            throw new InvalidOperationException("Missing config subcommand.");
        }

        switch (args[0].ToLowerInvariant())
        {
            case "set-pat":
                if (args.Length < 2)
                {
                    throw new InvalidOperationException("Usage: ado config set-pat <PAT>");
                }

                await _configCommands.SetPatAsync(config, args[1], cancellationToken);
                _host.Output.WriteSuccess("PAT updated.");
                return 0;

            case "set-org":
                await _configCommands.SetOrganizationAsync(config, args.Length > 1 ? args[1] : null, cancellationToken);
                _host.Output.WriteSuccess($"Current organization set to {config.CurrentContext.Organization}.");
                return 0;

            case "set-project":
                await _configCommands.SetProjectAsync(config, args.Length > 1 ? args[1] : null, cancellationToken);
                _host.Output.WriteSuccess($"Current project set to {config.CurrentContext.Project?.Name ?? config.CurrentContext.Project?.Id}.");
                return 0;

            case "set-repo":
                if (args.Length < 3)
                {
                    throw new InvalidOperationException("Usage: ado config set-repo <RepoName> <RepoPath>");
                }

                await _configCommands.SetRepositoryAsync(config, args[1], args[2], cancellationToken);
                _host.Output.WriteSuccess($"Current repository set to {config.CurrentContext.Repository?.Name}.");
                return 0;

            case "reset":
                await _configCommands.ResetAsync(config, cancellationToken);
                _host.Output.WriteSuccess("PAT and current org/project/repository context were reset.");
                return 0;

            default:
                throw new InvalidOperationException("Unknown config subcommand.");
        }
    }

    private async Task<int> HandleWorkItemAsync(Models.AppConfig config, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            throw new InvalidOperationException("Missing work-item subcommand.");
        }

        if (args[0].Equals("view", StringComparison.OrdinalIgnoreCase))
        {
            if (args.Length < 2 || !int.TryParse(args[1], out var viewWorkItemId) || viewWorkItemId <= 0)
            {
                throw new InvalidOperationException("Usage: ado work-item view <WorkItemId>");
            }

            var content = await _workItemHistoryCommands.ViewAsync(config, viewWorkItemId, cancellationToken);
            Console.WriteLine(content);
            return 0;
        }

        if (args[0].Equals("last", StringComparison.OrdinalIgnoreCase))
        {
            var last = _workItemHistoryCommands.GetLast(config);
            var content = await _workItemHistoryCommands.ViewAsync(config, last.Id, cancellationToken);
            Console.WriteLine(content);
            return 0;
        }

        if (!args[0].Equals("get", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Unknown work-item subcommand.");
        }

        if (args.Length < 2 || !int.TryParse(args[1], out var workItemId) || workItemId <= 0)
        {
            throw new InvalidOperationException("Usage: ado work-item get <WorkItemId> [--out <OutputPath>]");
        }

        string? outputOverride = null;
        if (args.Length > 2)
        {
            if (args.Length != 4 || !args[2].Equals("--out", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Usage: ado work-item get <WorkItemId> [--out <OutputPath>]");
            }

            outputOverride = args[3];
        }

        var artifacts = await _workItemCommands.GetAsync(config, workItemId, outputOverride, cancellationToken);
        _host.Output.WriteSuccess($"Work item artifacts written to {artifacts.JsonPath} and {artifacts.MarkdownPath}.");
        return 0;
    }

    private async Task<int> HandlePullRequestAsync(Models.AppConfig config, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            throw new InvalidOperationException("Missing pr subcommand.");
        }

        switch (args[0].ToLowerInvariant())
        {
            case "list-active":
            {
                var pullRequests = await _pullRequestCommands.ListActiveAsync(config, cancellationToken);
                foreach (var pullRequest in pullRequests)
                {
                    Console.WriteLine($"{pullRequest.Id}: {pullRequest.Title}");
                }

                return 0;
            }
            case "list-stored":
            {
                foreach (var entry in _pullRequestHistoryCommands.List(config))
                {
                    Console.WriteLine($"{entry.Id}: {entry.Title} [{entry.ReviewState}]");
                }

                return 0;
            }
            case "get":
            {
                var pullRequestId = ParsePositiveInt(args.ElementAtOrDefault(1), "Usage: ado pr get <PullRequestId>");
                var session = await _pullRequestCommands.GetAsync(config, pullRequestId, cancellationToken);
                _host.Output.WriteSuccess($"Pull request {session.PullRequestId} imported with {session.Threads.Count} threads.");
                return 0;
            }
            case "refresh":
            {
                var pullRequestId = ParsePositiveInt(args.ElementAtOrDefault(1), "Usage: ado pr refresh <PullRequestId>");
                var entry = _pullRequestHistoryCommands.Find(config, pullRequestId);
                var session = await _pullRequestCommands.RefreshAsync(config, entry, cancellationToken);
                _host.Output.WriteSuccess($"Pull request {session.PullRequestId} refreshed.");
                return 0;
            }
            case "threads":
            {
                var pullRequestId = ParsePositiveInt(args.ElementAtOrDefault(1), "Usage: ado pr threads <PullRequestId>");
                var entry = _pullRequestHistoryCommands.Find(config, pullRequestId);
                var session = await _pullRequestHistoryCommands.LoadSessionAsync(entry, cancellationToken);
                foreach (var thread in session.Threads.OrderBy(thread => thread.ThreadId))
                {
                    Console.WriteLine($"{thread.ThreadId}: {thread.Decision} {thread.FilePath} {FormatLine(thread.Line, thread.EndLine)}");
                }

                return 0;
            }
            case "thread":
                return await HandlePullRequestThreadAsync(config, args, cancellationToken);
            case "generate-prompt":
            {
                var pullRequestId = ParsePositiveInt(args.ElementAtOrDefault(1), "Usage: ado pr generate-prompt <PullRequestId>");
                var entry = _pullRequestHistoryCommands.Find(config, pullRequestId);
                var session = await _pullRequestHistoryCommands.LoadSessionAsync(entry, cancellationToken);
                await _pullRequestCommands.GeneratePromptAsync(config, session, cancellationToken);
                _host.Output.WriteSuccess($"Prompt generated for pull request {pullRequestId}.");
                return 0;
            }
            case "view":
            {
                var pullRequestId = ParsePositiveInt(args.ElementAtOrDefault(1), "Usage: ado pr view <PullRequestId>");
                var entry = _pullRequestHistoryCommands.Find(config, pullRequestId);
                var content = await _pullRequestCommands.ViewAsync(entry, cancellationToken);
                Console.WriteLine(content);
                return 0;
            }
            case "review":
            {
                var pullRequestId = ParsePositiveInt(args.ElementAtOrDefault(1), "Usage: ado pr review <PullRequestId>");
                var entry = _pullRequestHistoryCommands.Find(config, pullRequestId);
                var session = await _pullRequestHistoryCommands.LoadSessionAsync(entry, cancellationToken);
                var unreviewed = _pullRequestCommands.GetUnreviewedThreadIds(session);
                if (unreviewed.Count == 0)
                {
                    _host.Output.WriteInfo("Every thread is already reviewed.");
                    return 0;
                }

                var nextThread = session.Threads.First(thread => thread.ThreadId == unreviewed[0]);
                Console.WriteLine($"Next thread: {nextThread.ThreadId}");
                Console.WriteLine($"File: {nextThread.FilePath ?? "Not provided"}");
                Console.WriteLine($"Line: {FormatLine(nextThread.Line, nextThread.EndLine)}");
                foreach (var comment in nextThread.Comments)
                {
                    Console.WriteLine($"- {comment.Author}: {comment.Content}");
                }

                return 0;
            }
            default:
                throw new InvalidOperationException("Unknown pr subcommand.");
        }
    }

    private async Task<int> HandlePullRequestThreadAsync(Models.AppConfig config, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length < 3)
        {
            throw new InvalidOperationException("Usage: ado pr thread <PullRequestId> <ThreadId> [set fix|no-fix [--instruction <Instruction>]]");
        }

        var pullRequestId = ParsePositiveInt(args[1], "Usage: ado pr thread <PullRequestId> <ThreadId> ...");
        var threadId = ParsePositiveInt(args[2], "Usage: ado pr thread <PullRequestId> <ThreadId> ...");
        var entry = _pullRequestHistoryCommands.Find(config, pullRequestId);
        var session = await _pullRequestHistoryCommands.LoadSessionAsync(entry, cancellationToken);
        var thread = session.Threads.FirstOrDefault(item => item.ThreadId == threadId)
                     ?? throw new InvalidOperationException($"Thread {threadId} was not found.");

        if (args.Length == 3)
        {
            Console.WriteLine($"Thread {thread.ThreadId}: {thread.Decision}");
            Console.WriteLine($"File: {thread.FilePath ?? "Not provided"}");
            Console.WriteLine($"Line: {FormatLine(thread.Line, thread.EndLine)}");
            Console.WriteLine($"Comments: {thread.Comments.Count}");
            if (!string.IsNullOrWhiteSpace(thread.DeveloperNotes))
            {
                Console.WriteLine($"Instruction: {thread.DeveloperNotes}");
            }

            return 0;
        }

        if (args.Length < 5 || !args[3].Equals("set", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Usage: ado pr thread <PullRequestId> <ThreadId> set fix|no-fix [--instruction <Instruction>]");
        }

        var decision = args[4];
        string? instruction = null;
        if (decision.Equals("fix", StringComparison.OrdinalIgnoreCase) && args.Length > 5)
        {
            if (args.Length != 7 || !args[5].Equals("--instruction", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Usage: ado pr thread <PullRequestId> <ThreadId> set fix [--instruction <Instruction>]");
            }

            instruction = args[6];
        }

        await _pullRequestCommands.SetThreadDecisionAsync(config, session, threadId, decision, instruction, cancellationToken);
        _host.Output.WriteSuccess($"Thread {threadId} marked {decision}.");
        return 0;
    }

    private static int ParsePositiveInt(string? value, string usage)
    {
        if (!int.TryParse(value, out var parsed) || parsed <= 0)
        {
            throw new InvalidOperationException(usage);
        }

        return parsed;
    }

    private static string FormatLine(int? line, int? endLine)
    {
        if (line is null) return "n/a";
        return endLine is not null && endLine != line ? $"{line}-{endLine}" : line.Value.ToString();
    }
}
