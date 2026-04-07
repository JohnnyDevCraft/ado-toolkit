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
        AnsiConsole.WriteLine("  ado --help");
        AnsiConsole.WriteLine("  ado --version");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Notes[/]");
        AnsiConsole.WriteLine("  Running `set-org` or `set-project` without a value opens an interactive selector.");
    }
}
