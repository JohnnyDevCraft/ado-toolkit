using Spectre.Console;

namespace AdoToolkit.Commands;

public sealed class HelpCommand
{
    public void Execute()
    {
        AnsiConsole.MarkupLine("[bold]Usage[/]");
        AnsiConsole.WriteLine("  ado setup");
        AnsiConsole.WriteLine("  ado config set-pat <PAT>");
        AnsiConsole.WriteLine("  ado config set-org [OrgName]");
        AnsiConsole.WriteLine("  ado config set-project [ProjectId|ProjectName]");
        AnsiConsole.WriteLine("  ado config set-repo <RepoName> <RepoPath>");
        AnsiConsole.WriteLine("  ado config reset");
        AnsiConsole.WriteLine("  ado work-item get <WorkItemId> [--out <OutputPath>]");
        AnsiConsole.WriteLine("  ado work-item view <WorkItemId>");
        AnsiConsole.WriteLine("  ado work-item last");
        AnsiConsole.WriteLine("  ado pr list-active");
        AnsiConsole.WriteLine("  ado pr list-stored");
        AnsiConsole.WriteLine("  ado pr get <PullRequestId>");
        AnsiConsole.WriteLine("  ado pr refresh <PullRequestId>");
        AnsiConsole.WriteLine("  ado pr review <PullRequestId>");
        AnsiConsole.WriteLine("  ado pr threads <PullRequestId>");
        AnsiConsole.WriteLine("  ado pr thread <PullRequestId> <ThreadId>");
        AnsiConsole.WriteLine("  ado pr thread <PullRequestId> <ThreadId> set fix [--instruction <Instruction>]");
        AnsiConsole.WriteLine("  ado pr thread <PullRequestId> <ThreadId> set no-fix");
        AnsiConsole.WriteLine("  ado pr generate-prompt <PullRequestId>");
        AnsiConsole.WriteLine("  ado pr view <PullRequestId>");
        AnsiConsole.WriteLine("  ado --help");
        AnsiConsole.WriteLine("  ado --version");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Notes[/]");
        AnsiConsole.WriteLine("  Run `ado` with no arguments to open the interactive menu.");
        AnsiConsole.WriteLine("  Running `set-org` or `set-project` without a value opens an interactive selector.");
        AnsiConsole.WriteLine("  `config reset` clears PAT and current org/project/repository context but keeps stored work items and pull requests.");
    }
}
