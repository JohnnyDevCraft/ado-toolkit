using AdoToolkit.Models;
using AdoToolkit.Services;
using Spectre.Console;

namespace AdoToolkit.Presentation;

public sealed class MainMenuWorkflow
{
    private const string Exit = "Exit";
    private readonly PageLayout _pageLayout;
    private readonly CommandWorkflowBridge _bridge;
    private readonly ConsoleOutputService _output;

    public MainMenuWorkflow(PageLayout pageLayout, CommandWorkflowBridge bridge, ConsoleOutputService output)
    {
        _pageLayout = pageLayout;
        _bridge = bridge;
        _output = output;
    }

    public async Task RunAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            AnsiConsole.Clear();
            _pageLayout.Render("Main Menu", "Choose a workflow or drop straight to a direct command.");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Workflow")
                    .PageSize(12)
                    .AddChoices("Setup", "Work Items", "Pull Requests", "Reset Active Context", Exit));

            switch (choice)
            {
                case "Setup":
                    await RunSetupMenuAsync(config, cancellationToken);
                    break;
                case "Work Items":
                    await RunWorkItemMenuAsync(config, cancellationToken);
                    break;
                case "Pull Requests":
                    await RunPullRequestMenuAsync(config, cancellationToken);
                    break;
                case "Reset Active Context":
                    await _bridge.ResetAsync(config, cancellationToken);
                    _output.WriteSuccess("PAT and active organization, project, and repository were reset.");
                    Pause();
                    break;
                case Exit:
                    return;
            }
        }
    }

    private async Task RunSetupMenuAsync(AppConfig config, CancellationToken cancellationToken)
    {
        AnsiConsole.Clear();
        _pageLayout.Render("Setup", "Configure PAT, organization, project, and repository context.");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Setup Action")
                .AddChoices("Run Guided Setup", "Set PAT", "Set Organization", "Set Project", "Set Repository", "Back"));

        switch (choice)
        {
            case "Run Guided Setup":
                await _bridge.RunSetupAsync(config, cancellationToken);
                _output.WriteSuccess("Setup completed.");
                break;
            case "Set PAT":
            {
                var pat = AnsiConsole.Prompt(new TextPrompt<string>("Azure DevOps PAT").Secret());
                await _bridge.SetPatAsync(config, pat, cancellationToken);
                _output.WriteSuccess("PAT updated.");
                break;
            }
            case "Set Organization":
                await _bridge.SetOrganizationAsync(config, null, cancellationToken);
                _output.WriteSuccess($"Current organization set to {config.CurrentContext.Organization}.");
                break;
            case "Set Project":
                await _bridge.SetProjectAsync(config, null, cancellationToken);
                _output.WriteSuccess($"Current project set to {config.CurrentContext.Project?.Name}.");
                break;
            case "Set Repository":
            {
                var name = AnsiConsole.Ask<string>("Repository name");
                var path = AnsiConsole.Ask<string>("Repository path");
                await _bridge.SetRepositoryAsync(config, name, path, cancellationToken);
                _output.WriteSuccess($"Current repository set to {config.CurrentContext.Repository?.Name}.");
                break;
            }
            default:
                return;
        }

        Pause();
    }

    private async Task RunWorkItemMenuAsync(AppConfig config, CancellationToken cancellationToken)
    {
        AnsiConsole.Clear();
        _pageLayout.Render("Work Items", "Retrieve, list, and view stored work-item artifacts.");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Work Item Action")
                .AddChoices("Get Work Item", "List Stored Work Items", "View Last Work Item", "Back"));

        switch (choice)
        {
            case "Get Work Item":
            {
                var workItemId = AnsiConsole.Ask<int>("Work item ID");
                var artifacts = await _bridge.GetWorkItemAsync(config, workItemId, cancellationToken: cancellationToken);
                _output.WriteSuccess($"Work item artifacts written to {artifacts.JsonPath} and {artifacts.MarkdownPath}.");
                Pause();
                break;
            }
            case "List Stored Work Items":
                foreach (var entry in _bridge.ListWorkItems(config))
                {
                    AnsiConsole.WriteLine($"{entry.Id}: {entry.Title}");
                }
                Pause();
                break;
            case "View Last Work Item":
            {
                var last = _bridge.GetLastWorkItem(config);
                AnsiConsole.WriteLine(await _bridge.ViewWorkItemAsync(config, last.Id, cancellationToken));
                Pause();
                break;
            }
        }
    }

    private async Task RunPullRequestMenuAsync(AppConfig config, CancellationToken cancellationToken)
    {
        AnsiConsole.Clear();
        _pageLayout.Render("Pull Requests", "Import PRs, review threads, and generate fix prompts.");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Pull Request Action")
                .AddChoices("List My Active PRs", "Import PR By ID", "List Stored PRs", "Back"));

        switch (choice)
        {
            case "List My Active PRs":
                foreach (var pr in await _bridge.ListActivePullRequestsAsync(config, cancellationToken))
                {
                    AnsiConsole.WriteLine($"{pr.Id}: {pr.Title}");
                }
                Pause();
                break;
            case "Import PR By ID":
            {
                var pullRequestId = AnsiConsole.Ask<int>("Pull request ID");
                var session = await _bridge.GetPullRequestAsync(config, pullRequestId, cancellationToken);
                _output.WriteSuccess($"Pull request {session.PullRequestId} imported with {session.Threads.Count} threads.");
                Pause();
                break;
            }
            case "List Stored PRs":
            {
                var entries = _bridge.ListStoredPullRequests(config);
                foreach (var entry in entries)
                {
                    AnsiConsole.WriteLine($"{entry.Id}: {entry.Title} [{entry.ReviewState}]");
                }

                if (entries.Count > 0)
                {
                    var chosenPrId = AnsiConsole.Ask<int>("Pull request ID to inspect");
                    var entry = _bridge.FindStoredPullRequest(config, chosenPrId);
                    var session = await _bridge.LoadPullRequestSessionAsync(entry, cancellationToken);
                    await RunStoredPullRequestMenuAsync(config, entry, session, cancellationToken);
                }

                break;
            }
        }
    }

    private async Task RunStoredPullRequestMenuAsync(AppConfig config, Models.PullRequestEntry entry, Models.PullRequests.PullRequestSession session, CancellationToken cancellationToken)
    {
        while (true)
        {
            AnsiConsole.Clear();
            _pageLayout.Render($"PR {entry.Id}", entry.Title);
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Stored PR Action")
                    .AddChoices("View Threads", "Review Next Thread", "Generate Prompt", "View Preferred Artifact", "Back"));

            switch (choice)
            {
                case "View Threads":
                    foreach (var thread in session.Threads.OrderBy(thread => thread.ThreadId))
                    {
                        AnsiConsole.WriteLine($"{thread.ThreadId}: {thread.Decision} {thread.FilePath}");
                    }
                    Pause();
                    break;
                case "Review Next Thread":
                {
                    var pending = _bridge.GetUnreviewedThreadIds(session);
                    if (pending.Count == 0)
                    {
                        _output.WriteInfo("Every thread is already reviewed.");
                        Pause();
                        break;
                    }

                    var threadId = pending[0];
                    var decision = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title($"Thread {threadId} decision")
                            .AddChoices("fix", "no-fix", "cancel"));

                    if (decision == "cancel")
                    {
                        break;
                    }

                    string? instruction = null;
                    if (decision == "fix")
                    {
                        instruction = AnsiConsole.Prompt(new TextPrompt<string>("Fix instruction").AllowEmpty());
                    }

                    await _bridge.SetThreadDecisionAsync(config, session, threadId, decision, instruction, cancellationToken);
                    session = await _bridge.LoadPullRequestSessionAsync(entry, cancellationToken);
                    _output.WriteSuccess($"Thread {threadId} marked {decision}.");
                    Pause();
                    break;
                }
                case "Generate Prompt":
                    AnsiConsole.WriteLine(await _bridge.GeneratePromptAsync(config, session, cancellationToken));
                    Pause();
                    break;
                case "View Preferred Artifact":
                    AnsiConsole.WriteLine(await _bridge.ViewPullRequestAsync(entry, cancellationToken));
                    Pause();
                    break;
                case "Back":
                    return;
            }
        }
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine("[grey]Press enter to continue.[/]");
        Console.ReadLine();
    }
}
